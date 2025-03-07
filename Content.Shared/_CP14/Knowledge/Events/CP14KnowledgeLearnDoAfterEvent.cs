using Content.Shared._CP14.Knowledge.Prototypes;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Knowledge.Events;

[Serializable, NetSerializable]
public sealed partial class CP14KnowledgeLearnDoAfterEvent : DoAfterEvent
{
    public ProtoId<CP14KnowledgePrototype> Knowledge;

    public override DoAfterEvent Clone()
    {
        return this;
    }
}
