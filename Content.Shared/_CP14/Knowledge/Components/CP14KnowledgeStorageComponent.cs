using Content.Shared._CP14.Knowledge.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Knowledge.Components;

/// <summary>
/// A list of <see cref="CP14KnowledgePrototype"/> learned by this entity.
/// </summary>
[RegisterComponent, Access(typeof(SharedCP14KnowledgeSystem))]
public sealed partial class CP14KnowledgeStorageComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public HashSet<ProtoId<CP14KnowledgePrototype>> Knowledge { get; private set; } = [];
}
