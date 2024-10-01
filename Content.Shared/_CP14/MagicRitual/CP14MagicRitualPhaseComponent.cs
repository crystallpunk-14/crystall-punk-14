using Content.Shared._CP14.MagicRitual.Actions;
using Content.Shared._CP14.MagicRitual.Requirements;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitual;

/// <summary>
/// Magical entity that reacts to world events
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedRitualSystem))]
public sealed partial class CP14MagicRitualPhaseComponent : Component
{
    /// <summary>
    /// A link to the ritual itself in which this phase is found
    /// </summary>
    [DataField]
    public Entity<CP14MagicRitualComponent>? Ritual;

    [DataField]
    public Color PhaseColor = Color.White;

    [DataField(required: true)]
    public List<RitualPhaseEdge> Edges = new();
}

[DataRecord]
public partial record struct RitualPhaseEdge()
{
    public EntProtoId Target { get; set; }

    public List<CP14RitualRequirement> Requirements { get; set; } = new();

    public List<CP14RitualAction> Actions { get; set; } = new();
}
