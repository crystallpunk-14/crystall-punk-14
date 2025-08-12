using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitual.Components;

/// <summary>
///
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CP14RitualNodeComponent : Component
{
    [DataField]
    public List<CP14RitualEdge> Edges = new();
}


[DataDefinition]
public sealed partial class CP14RitualEdge
{
    [DataField(required: true)]
    public EntProtoId TargetNode = default!;

    [DataField]
    public List<CP14RitualCondition> Conditions = new();
}
