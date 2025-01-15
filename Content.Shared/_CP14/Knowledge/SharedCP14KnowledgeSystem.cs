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
        foreach (var knowledge in ent.Comp.Knowledge)
        {
            TryLearnKnowledge(ent, knowledge);
        }

        RemComp(ent, ent.Comp);
    }

    private void OnMapInit(Entity<CP14KnowledgeStorageComponent> ent, ref MapInitEvent args)
    {
        foreach (var knowledge in ent.Comp.Knowledges)
        {
            TryLearnKnowledge(ent, knowledge, force: true);
        }
    }

    public bool TryLearnKnowledge(EntityUid uid, ProtoId<CP14KnowledgePrototype> proto, bool force = false)
    {
        if (!TryComp<CP14KnowledgeStorageComponent>(uid, out var knowledgeStorage))
            return false;

        if (force)
        {
            if (!_proto.TryIndex(proto, out var indexedKnowledge))
                return false;

            //If we teach by force - we automatically teach all the basics that are necessary for that skill.
            foreach (var dependency in indexedKnowledge.Dependencies)
            {
                if (!TryLearnKnowledge(uid, dependency, true))
                    return false;
            }
        }

        return knowledgeStorage.Knowledges.Add(proto);
    }

    public bool TryForgotKnowledge(EntityUid uid, ProtoId<CP14KnowledgePrototype> proto)
    {
        if (!TryComp<CP14KnowledgeStorageComponent>(uid, out var knowledgeStorage))
            return false;

        if (!knowledgeStorage.Knowledges.Contains(proto))
            return false;

        knowledgeStorage.Knowledges.Remove(proto);

        return true;
    }

    public bool TryUseKnowledge(EntityUid uid, ProtoId<CP14KnowledgePrototype> knowledge, CP14KnowledgeStorageComponent? knowledgeStorage = null)
    {
        if (!Resolve(uid, ref knowledgeStorage, false))
            return false;

        if (!knowledgeStorage.Knowledges.Contains(knowledge))
            return false;

        var ev = new CP14KnowledgeUsedEvent(uid, knowledge);
        RaiseLocalEvent(uid, ev);
        return true;
    }
}

public sealed class CP14TrySkillIssueEvent : EntityEventArgs
{
    public readonly EntityUid User;

    public CP14TrySkillIssueEvent(EntityUid uid)
    {
        User = uid;
    }
}

public sealed class CP14KnowledgeUsedEvent : EntityEventArgs
{
    public readonly EntityUid User;
    public readonly ProtoId<CP14KnowledgePrototype> Knowledge;

    public CP14KnowledgeUsedEvent(EntityUid uid, ProtoId<CP14KnowledgePrototype> knowledge)
    {
        User = uid;
        Knowledge = knowledge;
    }
}
