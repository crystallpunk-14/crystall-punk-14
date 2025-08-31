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
using Content.Shared.Nutrition.EntitySystems;
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
/// Server-side staged butchering logic. It listens to sharp interactions and offers a verb,
/// but only for entities having CP14StagedButcherableComponent.
/// It does NOT modify vanilla ButcherableComponent behavior.
/// </summary>
public sealed class CP14StagedButcheringSystem : CP14SharedStagedButcheringSystem
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

        // Listen on TARGETS that can be staged-butchered (avoids duplicate subscription with SharpSystem).
        SubscribeLocalEvent<CP14StagedButcherableComponent, AfterInteractEvent>(
            OnAfterInteract,
            before: new[] { typeof(UtensilSystem) });

        // Keep DoAfter completion on sharp tools.
        SubscribeLocalEvent<SharpComponent, CP14ButcherStageDoAfterEvent>(OnDoAfter);

        SubscribeLocalEvent<CP14StagedButcherableComponent, GetVerbsEvent<InteractionVerb>>(OnGetVerbs);
    }


    // Handler must match: (EntityUid uid, TComp comp, ref TEvent args)
    private void OnAfterInteract(EntityUid uid, CP14StagedButcherableComponent staged, ref AfterInteractEvent args)
    {
        // uid is the TARGET entity (the one with CP14StagedButcherableComponent)
        if (args.Handled || !args.CanReach)
            return;

        // Only react when the player actually interacted with this uid.
        if (args.Target != uid)
            return;

        // Require a sharp tool in use.
        if (!TryComp(args.Used, out SharpComponent? sharp))
            return;

        // Start stage using the used tool (args.Used), the target (uid) and the user.
        if (TryStartStage(args.Used, uid, args.User, staged, sharp))
            args.Handled = true;
    }


    private void OnGetVerbs(EntityUid uid, CP14StagedButcherableComponent staged, GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        // allow using a sharp item in hand or the user itself (if it has SharpComponent)
        var hasSharpInHand = TryComp<SharpComponent>(args.Using, out var usingSharp);
        var hasSharpOnUser = TryComp<SharpComponent>(args.User, out var userSharp);

        var disabled = false;
        string? msg = null;

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

        // also check death requirement of CURRENT stage to pre-disable the verb
        if (!disabled && TryGetCurrentStage(staged, out var stage))
        {
            if (stage.RequireDead && TryComp<MobStateComponent>(uid, out var state) && !_mobState.IsDead(uid, state))
            {
                disabled = true;
                msg = Loc.GetString("butcherable-mob-isnt-dead");
            }
        }

        var sharpEnt = hasSharpInHand ? args.Using!.Value : (hasSharpOnUser ? args.User : EntityUid.Invalid);

        var verb = new InteractionVerb
        {
            Text = Loc.GetString("butcherable-verb-name"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/cutlery.svg.192dpi.png")),
            Disabled = disabled,
            Message = msg,
            Act = () =>
            {
                if (!disabled && sharpEnt != EntityUid.Invalid)
                {
                    TryComp(sharpEnt, out SharpComponent? sharp);
                    TryStartStage(sharpEnt, uid, args.User, staged, sharp);
                }
            }
        };

        args.Verbs.Add(verb);
    }

    private bool TryStartStage(EntityUid tool, EntityUid target, EntityUid user,
        CP14StagedButcherableComponent staged, SharpComponent? sharp)
    {
        if (staged.BeingButchered)
            return false;

        if (!TryGetCurrentStage(staged, out var stage))
            return false;

        // validate tool type: for now only Knife is triggered by SharpComponent interaction.
        if (stage.Tool != CP14ButcheringTool.Knife)
        {
            _popups.PopupEntity(Loc.GetString("butcherable-different-tool", ("target", target)), tool, user);
            return false;
        }

        // death requirement
        if (stage.RequireDead && TryComp<MobStateComponent>(target, out var state) && !_mobState.IsDead(target, state))
            return false;

        staged.BeingButchered = true;
        Dirty(target, staged);

        var needHand = user != tool;

        // apply tool modifier if Sharp system defines it
        var delay = stage.Delay;
        if (sharp != null)
            delay = sharp.ButcherDelayModifier * stage.Delay;

        var doAfter = new DoAfterArgs(EntityManager, user, delay, new CP14ButcherStageDoAfterEvent(), tool, target: target, used: tool)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = needHand
        };

        _doAfter.TryStartDoAfter(doAfter);
        return true;
    }

    private void OnDoAfter(EntityUid uid, SharpComponent sharp, DoAfterEvent ev)
    {
        if (ev.Handled || ev.Cancelled || ev.Args.Target is not EntityUid target)
            return;

        if (!TryComp<CP14StagedButcherableComponent>(target, out var staged))
            return;

        staged.BeingButchered = false;
        Dirty(target, staged);

        if (_containers.IsEntityInContainer(target))
        {
            ev.Handled = true;
            return;
        }

        if (!TryGetCurrentStage(staged, out var stage))
            return;

        // Spawn drops for this stage
        var spawns = EntitySpawnCollection.GetSpawns(stage.Spawned, _rand);
        var coords = _xform.GetMapCoordinates(target);
        EntityUid? popupEnt = null;

        foreach (var proto in spawns)
            popupEnt = Spawn(proto, coords.Offset(_rand.NextVector2(0.25f)));

        // Popup and sound (optional)
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

        // Advance stage pointer
        staged.CurrentStageIndex++;
        Dirty(target, staged);

        // Проверяем: был ли это последний stage
        if (staged.CurrentStageIndex >= staged.Stages.Count)
        {
            // optional gib for things with body
            if (stage.GibOnFinalize && TryComp<BodyComponent>(target, out var body))
                _body.GibBody(target, body: body);

            // destroy target (final butcher)
            EntityManager.DeleteEntity(target);
        }

        ev.Handled = true;

        _logs.Add(LogType.Gib,
            $"{ToPrettyString(ev.Args.User):user} staged-butchered {ToPrettyString(target):target} " +
            $"with {ToPrettyString(ev.Args.Used):tool} (stage {staged.CurrentStageIndex}/{staged.Stages.Count})");
    }

    private bool TryGetCurrentStage(CP14StagedButcherableComponent comp, out CP14ButcherStagePrototype? stage)
    {
        stage = null;

        if (comp.CurrentStageIndex < 0 || comp.CurrentStageIndex >= comp.Stages.Count)
            return false;

        return _protos.TryIndex(comp.Stages[comp.CurrentStageIndex], out stage);
    }
}
