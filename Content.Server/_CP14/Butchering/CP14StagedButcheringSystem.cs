using Content.Server.Body.Systems;
using Content.Server.Kitchen.Components;
using Content.Shared._CP14.Butchering;
using Content.Shared._CP14.Butchering.Components;
using Content.Shared._CP14.Butchering.Prototypes;
using Content.Shared.Administration.Logs;
using Content.Shared.Body.Components;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Kitchen;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Storage;
using Content.Shared.Verbs;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server._CP14.Butchering;

/// <summary>
/// Server-side staged butchering logic for CP14StagedButcherableComponent.
/// Integrates with SharpSystem and verb-based interaction.
/// Ensures cancelled DoAfter does not block retrying the stage.
/// </summary>
public sealed class CP14StagedButcheringSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;
    [Dependency] private readonly IPrototypeManager _protos = default!;
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly ContainerSystem _containers = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly ISharedAdminLogManager _logs = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Subscribe to DoAfter event specific for staged butchering
        // This prevents conflicts with SharpSystem or vanilla Butcherable
        SubscribeLocalEvent<SharpComponent, CP14ButcherStageDoAfterEvent>(OnDoAfter);

        // Add verbs for entities with staged butcherable component
        SubscribeLocalEvent<CP14StagedButcherableComponent, GetVerbsEvent<InteractionVerb>>(OnGetVerbs);
    }

    /// <summary>
    /// Entry point from SharpSystem for starting a stage.
    /// </summary>
    public bool TryStartStageFromSharp(EntityUid tool, EntityUid target, EntityUid user, SharpComponent sharp)
    {
        if (!TryComp(target, out CP14StagedButcherableComponent? staged))
            return false;

        // Delegate actual stage start to internal method
        return TryStartStage(tool, target, user, staged, sharp);
    }

    /// <summary>
    /// Adds the butcher verb to entities.
    /// Determines availability based on tool in hand, container state, and death requirement.
    /// </summary>
    private void OnGetVerbs(EntityUid uid, CP14StagedButcherableComponent staged, GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        // Check if user or their held item has a sharp component
        var hasSharpInHand = TryComp<SharpComponent>(args.Using, out var _);
        var hasSharpOnUser = TryComp<SharpComponent>(args.User, out var _);

        var disabled = false;
        string? msg = null;

        // Disable verb if no sharp tool available
        if (!hasSharpInHand && !hasSharpOnUser)
        {
            disabled = true;
            msg = Loc.GetString("butcherable-need-knife", ("target", uid));
        }
        else if (_containers.IsEntityInContainer(uid))
        {
            disabled = true;
            msg = Loc.GetString("butcherable-not-in-container", ("target", uid));
        }

        // Disable if stage requires dead target and target is alive
        if (!disabled && TryGetCurrentStage(staged, out var stage) && stage!.RequireDead &&
            TryComp<MobStateComponent>(uid, out var state) && !_mobState.IsDead(uid, state))
        {
            disabled = true;
            msg = Loc.GetString("butcherable-mob-isnt-dead");
        }

        // Determine which entity performs the action (tool in hand or user)
        var sharpEnt = hasSharpInHand ? args.Using!.Value : (hasSharpOnUser ? args.User : EntityUid.Invalid);

        // Create interaction verb
        var verb = new InteractionVerb
        {
            Text = Loc.GetString("butcherable-verb-name"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/cutlery.svg.192dpi.png")),
            Disabled = disabled,
            Message = msg,
            Act = () =>
            {
                if (disabled || sharpEnt == EntityUid.Invalid)
                    return;

                if (TryComp(sharpEnt, out SharpComponent? sharp))
                    TryStartStage(sharpEnt, uid, args.User, staged, sharp);
            }
        };

        args.Verbs.Add(verb);
    }

    /// <summary>
    /// Starts a butchering stage, checking conditions and scheduling DoAfter.
    /// </summary>
    private bool TryStartStage(EntityUid tool, EntityUid target, EntityUid user,
        CP14StagedButcherableComponent staged, SharpComponent? sharp)
    {
        // Prevent concurrent stage execution
        if (staged.BeingButchered)
            return false;

        if (!TryGetCurrentStage(staged, out var stage))
            return false;

        // Only knife can trigger a click-stage
        if (stage!.Tool != CP14ButcheringTool.Knife)
        {
            _popups.PopupEntity(Loc.GetString("butcherable-different-tool", ("target", target)), tool, user);
            return false;
        }

        // Check death requirement
        if (stage.RequireDead && TryComp<MobStateComponent>(target, out var state) && !_mobState.IsDead(target, state))
            return false;

        // Mark stage as in progress
        staged.BeingButchered = true;
        staged.StageInProgress = true;
        Dirty(target, staged);

        var needHand = user != tool;

        // Calculate delay, optionally modified by SharpComponent
        var delay = stage.Delay;
        if (sharp != null)
            delay = sharp.ButcherDelayModifier * stage.Delay;

        // Setup DoAfter for stage completion
        var doAfter = new DoAfterArgs(EntityManager, user, delay, new CP14ButcherStageDoAfterEvent(), tool, target: target, used: tool)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = needHand
        };

        _doAfter.TryStartDoAfter(doAfter);
        return true;
    }

    /// <summary>
    /// Handles completion or cancellation of staged butchering DoAfter.
    /// Spawns items, advances stage, and finalizes entity destruction if necessary.
    /// </summary>
    private void OnDoAfter(EntityUid uid, SharpComponent sharp, DoAfterEvent ev)
    {
        if (ev.Handled || ev.Args.Target is not EntityUid target)
            return;

        if (!TryComp<CP14StagedButcherableComponent>(target, out var staged))
            return;

        if (ev.Cancelled)
        {
            // Reset flags to allow retrying the stage
            staged.BeingButchered = false;
            staged.StageInProgress = false;
            Dirty(target, staged);
            ev.Handled = true;
            return;
        }

        staged.BeingButchered = false;
        staged.StageInProgress = false;
        Dirty(target, staged);

        // Prevent further processing if target is inside a container
        if (_containers.IsEntityInContainer(target))
        {
            ev.Handled = true;
            return;
        }

        if (!TryGetCurrentStage(staged, out var stage))
            return;

        // Spawn the items for this stage
        var spawns = EntitySpawnCollection.GetSpawns(stage!.Spawned, _rand);
        var coords = _xform.GetMapCoordinates(target);
        EntityUid? popupEnt = null;

        foreach (var proto in spawns)
            popupEnt = Spawn(proto, coords.Offset(_rand.NextVector2(0.25f)));

        // Show popup for spawned items
        if (popupEnt != null)
        {
            var text = stage.PopupOnSuccess != null
                ? Loc.GetString(stage.PopupOnSuccess.Value)
                : Loc.GetString("butcherable-knife-butchered-success",
                    ("target", target), ("knife", Identity.Entity(uid, EntityManager)));

            _popups.PopupEntity(text, popupEnt.Value, ev.Args.User);
        }

        // TODO sounds...
        // if (stage.SoundOnSuccess != null)
        //     SoundSystem.Play(stage.SoundOnSuccess.GetSound(), Filter.Pvs(target), target);
        // Advance stage index
        staged.CurrentStageIndex++;
        Dirty(target, staged);

        // Finalize entity if last stage
        if (staged.CurrentStageIndex >= staged.Stages.Count)
        {
            if (stage.GibOnFinalize && TryComp<BodyComponent>(target, out var body))
                _body.GibBody(target, body: body);

            EntityManager.DeleteEntity(target);
        }

        ev.Handled = true;

        // Log the butcher action for admin purposes
        _logs.Add(LogType.Gib,
            $"{ToPrettyString(ev.Args.User):user} staged-butchered {ToPrettyString(target):target} " +
            $"with {ToPrettyString(ev.Args.Used):tool} (stage {staged.CurrentStageIndex}/{staged.Stages.Count})");
    }

    /// <summary>
    /// Attempts to retrieve the current stage prototype from the staged component.
    /// </summary>
    private bool TryGetCurrentStage(CP14StagedButcherableComponent comp, out CP14ButcherStagePrototype? stage)
    {
        stage = null;

        if (comp.CurrentStageIndex < 0 || comp.CurrentStageIndex >= comp.Stages.Count)
            return false;

        return _protos.TryIndex(comp.Stages[comp.CurrentStageIndex], out stage);
    }
}
