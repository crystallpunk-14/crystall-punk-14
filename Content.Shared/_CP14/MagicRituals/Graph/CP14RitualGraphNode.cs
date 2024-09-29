
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRituals.Graph;

[Serializable]
[DataDefinition]
public sealed partial class CP14RitualGraphNode
{
    [DataField("node", required: true)]
    public string Name { get; private set; } = default!;

    [DataField(serverOnly: true)]
    public EntProtoId Entity { get; private set; }

    [DataField]
    public CP14RitualGraphEdge[] Edges = Array.Empty<CP14RitualGraphEdge>();
}
