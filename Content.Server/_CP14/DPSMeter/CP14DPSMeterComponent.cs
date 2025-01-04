using Content.Shared.Damage;

namespace Content.Server._CP14.DPSMeter;

[RegisterComponent]
public sealed partial class CP14DPSMeterComponent : Component
{
    [DataField]
    public DamageSpecifier TotalDamage = new DamageSpecifier();

    [DataField]
    public TimeSpan LastHitTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan StartTrackTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan EndTrackTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan TrackTimeAfterHit = TimeSpan.FromSeconds(5f);
}
