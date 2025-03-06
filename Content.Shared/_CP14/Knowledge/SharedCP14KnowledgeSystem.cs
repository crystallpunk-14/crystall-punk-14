using Content.Shared._CP14.Knowledge.Components;
using Content.Shared._CP14.Knowledge.Events;
using Content.Shared._CP14.Knowledge.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Knowledge;

public abstract class SharedCP14KnowledgeSystem : EntitySystem
{
    public bool HasKnowledge(Entity<CP14KnowledgeStorageComponent?> entity, ProtoId<CP14KnowledgePrototype> knowledge)
    {
        if (HasComp<CP14AllKnowingComponent>(entity))
            return true;

        return Resolve(entity, ref entity.Comp, false) && entity.Comp.Knowledge.Contains(knowledge);
    }

    public bool TryUseKnowledge(Entity<CP14KnowledgeStorageComponent?> entity, ProtoId<CP14KnowledgePrototype> knowledge, float factor = 1f)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return false;

        if (!entity.Comp.Knowledge.Contains(knowledge))
            return false;

        var ev = new CP14KnowledgeUsedEvent(entity, knowledge, factor);
        RaiseLocalEvent(entity, ev);

        return true;
    }
}
