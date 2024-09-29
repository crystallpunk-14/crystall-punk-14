using Content.Shared._CP14.MagicRituals.Requirements;

namespace Content.Shared._CP14.MagicRituals.Graph;

[Serializable]
[DataDefinition]
public sealed partial class CP14RitualGraphEdge
{
    [DataField("to", required:true)]
    public string Target { get; private set; } = string.Empty;

    [DataField]
    public CP14RitualRequirement[] Requirements = Array.Empty<CP14RitualRequirement>();
}
