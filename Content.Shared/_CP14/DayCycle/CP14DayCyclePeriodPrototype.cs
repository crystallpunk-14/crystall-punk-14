using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.DayCycle;

[Prototype("CP14DayCyclePeriod")]
public sealed class CP14DayCyclePeriodPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = string.Empty;
}
