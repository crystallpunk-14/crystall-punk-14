using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Knowledge.Events;

[Serializable, NetSerializable]
public sealed class CP14RequestKnowledgeInfoEvent : EntityEventArgs
{
    public readonly NetEntity NetEntity;

    public CP14RequestKnowledgeInfoEvent(NetEntity netEntity)
    {
        NetEntity = netEntity;
    }
}
