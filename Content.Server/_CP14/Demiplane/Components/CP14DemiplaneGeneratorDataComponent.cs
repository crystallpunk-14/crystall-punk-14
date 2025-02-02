using Content.Shared._CP14.Demiplane.Prototypes;
using Content.Shared._CP14.RoundStatistic;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Demiplane.Components;

/// <summary>
/// Stores the data needed to generate a new demiplane
/// </summary>
[RegisterComponent, Access(typeof(CP14DemiplaneSystem))]
public sealed partial class CP14DemiplaneGeneratorDataComponent : Component
{
    [DataField]
    public ProtoId<CP14DemiplaneLocationPrototype>? Location;

    [DataField]
    public List<ProtoId<CP14DemiplaneModifierPrototype>> SelectedModifiers = new();

    /// <summary>
    /// Generator Tier. Determines which modifiers and locations will be selected for this demiplane
    /// </summary>
    [DataField(required: true)]
    public Dictionary<int, float> TiersContent = new();

    [DataField(required: true)]
    public Dictionary<ProtoId<CP14DemiplaneModifierCategoryPrototype>, float> Limits = new();

    [DataField]
    public ProtoId<CP14RoundStatTrackerPrototype> Statistic = "DemiplaneOpen";
}
