using Content.Shared._CP14.Skills.Components;
using Content.Shared._CP14.Skills.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._CP14.Skills;

public partial class SharedCP14SkillSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CP14SkillsStorageComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CP14AutoAddSkillComponent, MapInitEvent>(AutoAddSkill);
    }

    private void AutoAddSkill(Entity<CP14AutoAddSkillComponent> ent, ref MapInitEvent args)
    {
        foreach (var skill in ent.Comp.Skills)
        {
            TryLearnSkill(ent, skill);
        }

        RemComp(ent, ent.Comp);
    }

    private void OnMapInit(Entity<CP14SkillsStorageComponent> ent, ref MapInitEvent args)
    {
        foreach (var skill in ent.Comp.Skills)
        {
            TryLearnSkill(ent, skill, force: true);
        }
    }

    public bool TryLearnSkill(EntityUid uid, ProtoId<CP14SkillPrototype> skill, bool force = false)
    {
        if (!TryComp<CP14SkillsStorageComponent>(uid, out var skillStorage))
            return false;

        if (!skillStorage.Skills.Contains(skill))
        {
            skillStorage.Skills.Add(skill);
            if (!force)
                return false;
        }

        var proto = _proto.Index(skill);
        EntityManager.AddComponents(uid, proto.Components);

        return true;
    }

    public bool TryForgotSkill(EntityUid uid, ProtoId<CP14SkillPrototype> skill)
    {
        if (!TryComp<CP14SkillsStorageComponent>(uid, out var skillStorage))
            return false;

        if (!skillStorage.Skills.Contains(skill))
            return false;

        skillStorage.Skills.Remove(skill);

        var proto = _proto.Index(skill);
        EntityManager.RemoveComponents(uid, proto.Components);

        return true;
    }
}

public sealed partial class CP14TrySkillIssueEvent : EntityEventArgs
{
    public readonly EntityUid User;

    public CP14TrySkillIssueEvent(EntityUid uid)
    {
        User = uid;
    }
}
