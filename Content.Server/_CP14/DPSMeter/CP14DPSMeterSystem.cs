using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Server._CP14.DPSMeter;

public sealed class CP14DPSMeterSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14DPSMeterComponent, DamageChangedEvent>(OnDamageChanged);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14DPSMeterComponent>();
        while (query.MoveNext(out var uid, out var meter))
        {
            if (_timing.CurTime < meter.EndTrackTime || meter.EndTrackTime == TimeSpan.Zero)
                continue;

            //Clear tracking
            _popup.PopupEntity($"TOTAL DPS: {GetDPS((uid, meter))}", uid, PopupType.Large);
            meter.TotalDamage = new DamageSpecifier();
            meter.EndTrackTime = TimeSpan.Zero;
            meter.StartTrackTime = TimeSpan.Zero;
        }
    }

    private void OnDamageChanged(Entity<CP14DPSMeterComponent> ent, ref DamageChangedEvent args)
    {
        if (args.DamageDelta is null)
            return;

        ent.Comp.TotalDamage += args.DamageDelta;

        if (ent.Comp.StartTrackTime == TimeSpan.Zero)
            ent.Comp.StartTrackTime = _timing.CurTime;

        ent.Comp.LastHitTime = _timing.CurTime;
        ent.Comp.EndTrackTime = _timing.CurTime + ent.Comp.TrackTimeAfterHit;

        _popup.PopupEntity($"DPS: {GetDPS(ent)}", ent);
    }

    private FixedPoint2 GetDPS(Entity<CP14DPSMeterComponent> ent)
    {
        var totalDamage = ent.Comp.TotalDamage.GetTotal();
        var totalSeconds = (ent.Comp.LastHitTime - ent.Comp.StartTrackTime).TotalSeconds;

        var DPS = totalDamage / Math.Min(totalSeconds, 1f);

        return DPS;
    }
}
