using Content.Shared.Whitelist;

namespace Content.Shared._CP14.Demiplan.Components;

/// <summary>
/// teleports a certain number of entities between demiplanes with a delay
/// </summary>
[RegisterComponent]
public sealed partial class CP14DemiplanRadiusTimedPasswayComponent : Component
{
    [DataField]
    public int MaxEntities = 3;

    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(10f);

    [DataField]
    public float Radius = 3f;

    [DataField]
    public TimeSpan NextTimeTeleport = TimeSpan.Zero;

    [DataField]
    public bool Enabled = true;
}
