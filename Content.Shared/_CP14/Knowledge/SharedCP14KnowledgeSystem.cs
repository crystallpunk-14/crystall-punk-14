using Content.Shared._CP14.Knowledge.Components;
using Content.Shared._CP14.Knowledge.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._CP14.Knowledge;

public abstract partial class SharedCP14KnowledgeSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CP14KnowledgeStorageComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CP14AutoAddKnowledgeComponent, MapInitEvent>(AutoAddSkill);
    }

    private void AutoAddSkill(Entity<CP14AutoAddKnowledgeComponent> ent, ref MapInitEvent args)
    {
        foreach (var skill in ent.Comp.Knowledge)
        {
            TryAddKnowledge(ent, skill);
        }

        RemComp(ent, ent.Comp);
    }

    private void OnMapInit(Entity<CP14KnowledgeStorageComponent> ent, ref MapInitEvent args)
    {
        foreach (var skill in ent.Comp.Skills)
        {
            TryAddKnowledge(ent, skill, force: true);
        }
    }

    public bool TryAddKnowledge(EntityUid uid, ProtoId<CP14KnowledgePrototype> skill, bool force = false)
    {
        if (!TryComp<CP14KnowledgeStorageComponent>(uid, out var skillStorage))
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

    public bool TryForgotSkill(EntityUid uid, ProtoId<CP14KnowledgePrototype> skill)
    {
        if (!TryComp<CP14KnowledgeStorageComponent>(uid, out var skillStorage))
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
