using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.RoundStatistic;

[Prototype("statisticTracker")]
public sealed partial class CP14RoundStatTrackerPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    [DataField(required: true)]
    public LocId Text;
}
