using Content.Shared._CP14.Skill;
using Content.Shared._CP14.Skill.Components;
using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared._CP14.Vampire.Components;
using Content.Shared.Actions;
using Content.Shared.Body.Systems;
using Content.Shared.Buckle.Components;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Humanoid;
using Content.Shared.Jittering;
using Content.Shared.Popups;
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
    [Dependency] protected readonly IPrototypeManager Proto = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private readonly ProtoId<CP14SkillPointPrototype> _skillPointType = "Blood";
    private readonly ProtoId<CP14SkillPointPrototype> _memorySkillPointType = "Memory";

    public override void Initialize()
    {
        base.Initialize();
        InitializeSpell();

        SubscribeLocalEvent<CP14VampireComponent, MapInitEvent>(OnVampireInit);
        SubscribeLocalEvent<CP14VampireComponent, ComponentRemove>(OnVampireRemove);

        SubscribeLocalEvent<CP14VampireComponent, CP14ToggleVampireVisualsAction>(OnToggleVisuals);
        SubscribeLocalEvent<CP14VampireComponent, CP14VampireToggleVisualsDoAfter>(OnToggleDoAfter);

        SubscribeLocalEvent<CP14VampireVisualsComponent, ComponentInit>(OnVampireVisualsInit);
        SubscribeLocalEvent<CP14VampireVisualsComponent, ComponentShutdown>(OnVampireVisualsShutdown);
        SubscribeLocalEvent<CP14VampireVisualsComponent, ExaminedEvent>(OnVampireExamine);

        SubscribeLocalEvent<CP14VampireEssenceHolderComponent, ExaminedEvent>(OnEssenceHolderExamined);
    }

    private void OnEssenceHolderExamined(Entity<CP14VampireEssenceHolderComponent> ent, ref ExaminedEvent args)
    {
        if (!HasComp<CP14ShowVampireEssenceComponent>(args.Examiner))
            return;

        if (!args.IsInDetailsRange)
            return;

        args.PushMarkup(Loc.GetString("cp14-vampire-essence-holder-examine", ("essence", ent.Comp.Essence)));
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

        //Skill tree base nerf
        _skill.RemoveSkillPoints(ent, _memorySkillPointType, 2, true);

        //Remove blood essence
        if (TryComp<CP14VampireEssenceHolderComponent>(ent, out var essenceHolder))
        {
            essenceHolder.Essence = 0;
            Dirty(ent, essenceHolder);
        }
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
                if (!Proto.TryIndex(skill, out var indexedSkill))
                    continue;

                if (indexedSkill.Tree == ent.Comp.SkillTreeProto)
                    _skill.TryRemoveSkill(ent, skill);
            }
        }
        _skill.RemoveSkillPoints(ent, ent.Comp.SkillPointProto, ent.Comp.SkillPointCount);
        _skill.AddSkillPoints(ent, _memorySkillPointType, 2, null, true);
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

    public void GatherEssence(Entity<CP14VampireComponent?> vampire,
        Entity<CP14VampireEssenceHolderComponent?> victim,
        FixedPoint2 amount)
    {
        if (!Resolve(vampire, ref vampire.Comp, false))
            return;

        if (!Resolve(victim, ref victim.Comp, false))
            return;

        var extractedEssence = MathF.Min(victim.Comp.Essence.Float(), amount.Float());

        if (TryComp<BuckleComponent>(victim, out var buckle) && buckle.BuckledTo is not null)
        {
            if (TryComp<CP14VampireAltarComponent>(buckle.BuckledTo, out var altar))
            {
                extractedEssence *= altar.Multiplier;
            }
        }

        if (extractedEssence <= 0)
        {
            _popup.PopupClient(Loc.GetString("cp14-vampire-gather-essence-no-left"), victim, vampire, PopupType.SmallCaution);
            return;
        }

        _skill.AddSkillPoints(vampire, _skillPointType, extractedEssence);
        victim.Comp.Essence -= amount;

        Dirty(victim);
    }
}


public sealed partial class CP14ToggleVampireVisualsAction : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class CP14VampireToggleVisualsDoAfter : SimpleDoAfterEvent;


// Appearance Data key
[Serializable, NetSerializable]
public enum VampireClanLevelVisuals : byte
{
    Level,
}
