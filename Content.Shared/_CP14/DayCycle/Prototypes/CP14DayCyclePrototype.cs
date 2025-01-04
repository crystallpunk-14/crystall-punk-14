using Content.Shared._CP14.DayCycle.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.DayCycle.Prototypes;

[Prototype("CP14DayCycle")]
public sealed class CP14DayCyclePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = string.Empty;

    [DataField(required: true), ViewVariables]
    public List<DayCycleEntry> TimeEntries = new();
}
