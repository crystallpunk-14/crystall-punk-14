using Content.Shared._CP14.Skill;
using Content.Shared._CP14.Skill.Components;
using Content.Shared.Actions;
using Content.Shared.Body.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Humanoid;
using Content.Shared.Jittering;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.Vampire;

public abstract class CP14SharedVampireSystem : EntitySystem
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

        SubscribeLocalEvent<CP14VampireComponent, MapInitEvent>(OnVampireInit);
        SubscribeLocalEvent<CP14VampireComponent, ComponentRemove>(OnVampireRemove);

        SubscribeLocalEvent<CP14VampireComponent, CP14ToggleVampireVisualsAction>(OnToggleVisuals);
        SubscribeLocalEvent<CP14VampireComponent, CP14VampireToggleVisualsDoAfter>(OnToggleDoAfter);

        SubscribeLocalEvent<CP14VampireVisualsComponent, ComponentInit>(OnVampireVisualsInit);
        SubscribeLocalEvent<CP14VampireVisualsComponent, ComponentShutdown>(OnVampireVisualsShutdown);
        SubscribeLocalEvent<CP14VampireVisualsComponent, ExaminedEvent>(OnVampireExamine);
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
    }

    private void OnToggleVisuals(Entity<CP14VampireComponent> ent, ref CP14ToggleVampireVisualsAction args)
    {
        if (_timing.IsFirstTimePredicted)
            _jitter.DoJitter(ent, ent.Comp.ToggleVisualsTime, true);

        var doAfterArgs = new DoAfterArgs(EntityManager,
            ent,
            ent.Comp.ToggleVisualsTime,
            new CP14VampireToggleVisualsDoAfter(),
            ent)
        {
            Hidden = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnToggleDoAfter(Entity<CP14VampireComponent> ent, ref CP14VampireToggleVisualsDoAfter args)
    {
        if (HasComp<CP14VampireVisualsComponent>(ent))
        {
            RemCompDeferred<CP14VampireVisualsComponent>(ent);
        }
        else
        {
            EnsureComp<CP14VampireVisualsComponent>(ent);
        }
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
