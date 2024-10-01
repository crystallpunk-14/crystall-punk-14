using Content.Server._CP14.MagicRituals.Components.Actions;
using Content.Server._CP14.MagicRituals.Components.Requirements;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.MagicRituals.Components;

/// <summary>
/// Magical entity that reacts to world events
/// </summary>
[RegisterComponent, Access(typeof(CP14RitualSystem))]
public sealed partial class CP14MagicRitualPhaseComponent : Component
{
    /// <summary>
    ///
    /// </summary>
    [DataField]
    public Entity<CP14MagicRitualComponent>? Ritual;

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
