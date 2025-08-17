using Content.Shared._CP14.MagicSpell.Events;
using Content.Shared._CP14.Skill;
using Content.Shared._CP14.Skill.Components;
using Content.Shared._CP14.Transmutation.Components;
using Content.Shared._CP14.Vampire.Components;
using Content.Shared.Actions;
using Content.Shared.Body.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Humanoid;
using Content.Shared.Jittering;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.Vampire;

public abstract partial class CP14SharedVampireSystem : EntitySystem
{
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly CP14SharedSkillSystem _skill = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeTree();

        SubscribeLocalEvent<CP14VampireComponent, MapInitEvent>(OnVampireInit);
        SubscribeLocalEvent<CP14VampireComponent, ComponentRemove>(OnVampireRemove);

        SubscribeLocalEvent<CP14VampireComponent, CP14ToggleVampireVisualsAction>(OnToggleVisuals);
        SubscribeLocalEvent<CP14VampireComponent, CP14VampireToggleVisualsDoAfter>(OnToggleDoAfter);

        SubscribeLocalEvent<CP14VampireVisualsComponent, ComponentInit>(OnVampireVisualsInit);
        SubscribeLocalEvent<CP14VampireVisualsComponent, ComponentShutdown>(OnVampireVisualsShutdown);
        SubscribeLocalEvent<CP14VampireVisualsComponent, ExaminedEvent>(OnVampireExamine);

        SubscribeLocalEvent<CP14MagicEffectVampireComponent, CP14CastMagicEffectAttemptEvent>(OnVampireCastAttempt);
        SubscribeLocalEvent<CP14MagicEffectVampireComponent, ExaminedEvent>(OnVampireCastExamine);

        SubscribeLocalEvent<CP14TransmutableComponent, ExaminedEvent>(OnTransmutableExamined);
    }

    private void OnTransmutableExamined(Entity<CP14TransmutableComponent> ent, ref ExaminedEvent args)
    {
        if (!TryComp<CP14VampireComponent>(args.Examiner, out var vampire))
            return;

        if (!_proto.TryIndex(vampire.Faction, out var indexedFaction))
            return;

        if (indexedFaction.TransmutationMethod is null)
            return;

        var entries = ent.Comp.Entries;
        if (!entries.TryGetValue(indexedFaction.TransmutationMethod.Value, out var targetProto))
            return;

        if (!_proto.TryIndex(targetProto, out var indexedTargetProto))
            return;

        var name = indexedTargetProto.Name;

        args.PushMarkup(Loc.GetString("cp-14-vampire-transmutable-to", ("name", name), ("count", ent.Comp.Cost)));
    }

    private void OnVampireCastExamine(Entity<CP14MagicEffectVampireComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup($"{Loc.GetString("cp14-magic-spell-need-vampire-valid")}", priority: 10);
    }

    private void OnVampireCastAttempt(Entity<CP14MagicEffectVampireComponent> ent, ref CP14CastMagicEffectAttemptEvent args)
    {
        //If we are not vampires in principle, we certainly should not have this ability,
        //but then we will not limit its use to a valid vampire form that is unavailable to us.

        if (!HasComp<CP14VampireComponent>(args.Performer))
            return;

        if (!HasComp<CP14VampireVisualsComponent>(args.Performer))
        {
            args.PushReason(Loc.GetString("cp14-magic-spell-need-vampire-valid"));
            args.Cancel();
        }
    }

    protected virtual void OnVampireInit(Entity<CP14VampireComponent> ent, ref MapInitEvent args)
    {
        //Bloodstream
        _bloodstream.ChangeBloodReagent(ent.Owner, ent.Comp.NewBloodReagent);

        //Actions
        foreach (var proto in ent.Comp.ActionsProto)
        {
            EntityUid? newAction = null;
            _action.AddAction(ent, ref newAction, proto);
        }

        //Skill tree
        _skill.AddSkillPoints(ent, ent.Comp.SkillPointProto, ent.Comp.SkillPointCount, silent: true);
        _skill.AddSkillTree(ent, ent.Comp.SkillTreeProto);
    }

    private void OnVampireRemove(Entity<CP14VampireComponent> ent, ref ComponentRemove args)
    {
        RemCompDeferred<CP14VampireVisualsComponent>(ent);

        //Bloodstream todo

        //Metabolism todo

        //Actions
        foreach (var action in ent.Comp.Actions)
        {
            _action.RemoveAction(ent.Owner, action);
        }

        //Skill tree
        _skill.RemoveSkillTree(ent, ent.Comp.SkillTreeProto);
        if (TryComp<CP14SkillStorageComponent>(ent, out var storage))
        {
            foreach (var skill in storage.LearnedSkills)
            {
                if (!_proto.TryIndex(skill, out var indexedSkill))
                    continue;

                if (indexedSkill.Tree == ent.Comp.SkillTreeProto)
                    _skill.TryRemoveSkill(ent, skill);
            }
        }
        _skill.RemoveSkillPoints(ent, ent.Comp.SkillPointProto, ent.Comp.SkillPointCount);
    }

    private void OnToggleVisuals(Entity<CP14VampireComponent> ent, ref CP14ToggleVampireVisualsAction args)
    {
        if (_timing.IsFirstTimePredicted)
            _jitter.DoJitter(ent, ent.Comp.ToggleVisualsTime, true);

        var doAfterArgs = new DoAfterArgs(EntityManager, ent, ent.Comp.ToggleVisualsTime, new CP14VampireToggleVisualsDoAfter(), ent)
        {
            Hidden = true,
            NeedHand = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnToggleDoAfter(Entity<CP14VampireComponent> ent, ref CP14VampireToggleVisualsDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (HasComp<CP14VampireVisualsComponent>(ent))
        {
            RemCompDeferred<CP14VampireVisualsComponent>(ent);
        }
        else
        {
            EnsureComp<CP14VampireVisualsComponent>(ent);
        }

        args.Handled = true;
    }

    protected virtual void OnVampireVisualsShutdown(Entity<CP14VampireVisualsComponent> vampire, ref ComponentShutdown args)
    {
        if (!EntityManager.TryGetComponent(vampire, out HumanoidAppearanceComponent? humanoidAppearance))
            return;

        humanoidAppearance.EyeColor = vampire.Comp.OriginalEyesColor;

        Dirty(vampire, humanoidAppearance);
    }

    protected virtual void OnVampireVisualsInit(Entity<CP14VampireVisualsComponent> vampire, ref ComponentInit args)
    {
        if (!EntityManager.TryGetComponent(vampire, out HumanoidAppearanceComponent? humanoidAppearance))
            return;

        vampire.Comp.OriginalEyesColor = humanoidAppearance.EyeColor;
        humanoidAppearance.EyeColor = vampire.Comp.EyesColor;

        Dirty(vampire, humanoidAppearance);
    }

    private void OnVampireExamine(Entity<CP14VampireVisualsComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("cp14-vampire-examine"));
    }
}


public sealed partial class CP14ToggleVampireVisualsAction : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class CP14VampireToggleVisualsDoAfter : SimpleDoAfterEvent;
