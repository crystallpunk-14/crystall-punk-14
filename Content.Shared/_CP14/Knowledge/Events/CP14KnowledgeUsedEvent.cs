using Content.Shared._CP14.Knowledge.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Knowledge.Events;

public sealed class CP14KnowledgeUsedEvent : EntityEventArgs
{
    public readonly EntityUid User;
    public readonly ProtoId<CP14KnowledgePrototype> Knowledge;
    public readonly float Factor;

    public CP14KnowledgeUsedEvent(EntityUid uid, ProtoId<CP14KnowledgePrototype> knowledge, float factor)
    {
        User = uid;
        Knowledge = knowledge;
        Factor = factor;
    }
}
