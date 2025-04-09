using Content.Shared._CP14.RoundStatistic;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.RoundStatistic.DemiplaneDeath;

/// <summary>
/// Tracks the destruction or full-blown death of this entity.
/// </summary>
[RegisterComponent]
public sealed partial class CP14DeathDemiplaneStatisticComponent : Component
{
    [DataField]
    public ProtoId<CP14RoundStatTrackerPrototype> Statistic = "DemiplaneDeaths";
}
