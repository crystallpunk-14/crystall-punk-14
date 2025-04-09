namespace Content.Shared._CP14.Demiplane.Components;

/// <summary>
/// Keeps the demiplanes from being destroyed while they're in it.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedDemiplaneSystem))]
public sealed partial class CP14DemiplaneStabilizerComponent : Component
{
    /// <summary>
    /// must be a being and be alive to work as a stabilizer
    /// </summary>
    [DataField]
    public bool RequireAlive = false;

    [DataField]
    public bool Enabled = true;
}
