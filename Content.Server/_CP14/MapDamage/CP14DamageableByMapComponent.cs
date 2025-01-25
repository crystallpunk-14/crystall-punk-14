namespace Content.Server._CP14.MapDamage;

/// <summary>
/// can take damage from being directly on the map (not on the grid)
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class CP14DamageableByMapComponent : Component
{
    [DataField, AutoPausedField]
    public TimeSpan NextDamageTime = TimeSpan.Zero;

    [DataField]
    public bool Enabled = false;

    [DataField]
    public EntityUid? CurrentMap;
}
