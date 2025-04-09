using System.Numerics;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Temperature;

/// <summary>
/// passively returns the solution temperature to the standard
/// </summary>
[RegisterComponent, Access(typeof(CP14TemperatureSystem))]
public sealed partial class CP14TemperatureTransformationComponent : Component
{
    [DataField(required: true)]
    public List<CP14TemperatureTransformEntry> Entries = new();

    /// <summary>
    /// solution where reagents will be added from newly added ingredients
    /// </summary>
    [DataField]
    public string Solution = "food";
}

[DataRecord]
public record struct CP14TemperatureTransformEntry()
{
    public EntProtoId? TransformTo { get; set; } = null;
    public Vector2 TemperatureRange { get; set; } = new();
}
