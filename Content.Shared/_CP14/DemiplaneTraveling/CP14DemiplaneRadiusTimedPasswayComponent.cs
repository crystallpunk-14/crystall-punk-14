namespace Content.Shared._CP14.DemiplaneTraveling;

/// <summary>
/// teleports a certain number of entities between demiplanes with a delay
/// </summary>
[RegisterComponent]
public sealed partial class CP14DemiplaneRadiusTimedPasswayComponent : Component
{
    [DataField]
    public int MaxEntities = 3;

    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(10f);

    [DataField]
    public float Radius = 3f;

    [DataField]
    public TimeSpan NextTimeTeleport = TimeSpan.Zero;
}
