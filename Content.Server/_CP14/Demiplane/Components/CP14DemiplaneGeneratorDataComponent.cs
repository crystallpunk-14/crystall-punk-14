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
    /// Demiplane Difficulty Level. By design, the plan so far is for a framework of 1 to 10, but technically could support more.
    /// </summary>
    [DataField(required: true)]
    public int Level = 1;

    [DataField(required: true)]
    public Dictionary<ProtoId<CP14DemiplaneModifierCategoryPrototype>, float> Limits = new();

    [DataField]
    public ProtoId<CP14RoundStatTrackerPrototype> Statistic = "DemiplaneOpen";

    [DataField]
    public List<EntProtoId> AutoRifts = new() { "CP14DemiplaneTimedRadiusPassway", "CP14DemiplanRiftCore" };
}
