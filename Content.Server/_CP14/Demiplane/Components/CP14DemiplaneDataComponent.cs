using Content.Server._CP14.DemiplaneTraveling;
using Content.Shared._CP14.Demiplane.Prototypes;
using Content.Shared._CP14.RoundStatistic;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Demiplane.Components;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CP14DemiplaneSystem), typeof(CP14StationDemiplaneMapSystem))]
public sealed partial class CP14DemiplaneDataComponent : Component
{
    [DataField]
    public ProtoId<CP14DemiplaneLocationPrototype>? Location;

    [DataField]
    public List<ProtoId<CP14DemiplaneModifierPrototype>> SelectedModifiers = new();

    [DataField]
    public ProtoId<CP14RoundStatTrackerPrototype> Statistic = "DemiplaneOpen";

    [DataField]
    public List<EntProtoId> AutoRifts = new() { "CP14DemiplaneTimedRadiusPassway", "CP14DemiplanRiftCore" };
}
