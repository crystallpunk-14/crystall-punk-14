using Content.Shared.Chemistry.Components;

namespace Content.Server._CP14.Temperature;

/// <summary>
///
/// </summary>

[RegisterComponent, Access(typeof(CPMeltingSystem))]
public sealed partial class CPMeltingComponent : Component
{
    [DataField]
    public Solution? MeltSolution;

    [DataField]
    public float MeltTemperature = 1000f;
}
