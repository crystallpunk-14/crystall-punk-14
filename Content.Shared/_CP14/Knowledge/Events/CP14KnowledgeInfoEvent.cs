using Content.Shared._CP14.Knowledge.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Knowledge.Events;

[Serializable, NetSerializable]
public sealed class CP14KnowledgeInfoEvent : EntityEventArgs
{
    public readonly NetEntity NetEntity;
    public readonly HashSet<ProtoId<CP14KnowledgePrototype>> AllKnowledge;

    public CP14KnowledgeInfoEvent(NetEntity netEntity, HashSet<ProtoId<CP14KnowledgePrototype>> allKnowledge)
    {
        NetEntity = netEntity;
        AllKnowledge = allKnowledge;
    }
}
