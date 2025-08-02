namespace Content.Server._CP14.GameTicking.Rules.Components;

/// <summary>
/// A rule that assigns common goals to different roles. Common objectives are generated once at the beginning of a round and are shared between players.
/// </summary>
[RegisterComponent, Access(typeof(CP14CrashingShipRule))]
public sealed partial class CP14CrashingShipRuleComponent : Component
{
    [DataField]
    public EntityUid? Ship;

    [DataField]
    public bool PendingExplosions = true;

    [DataField]
    public TimeSpan StartExplosionTime = TimeSpan.FromMinutes(1);
}
