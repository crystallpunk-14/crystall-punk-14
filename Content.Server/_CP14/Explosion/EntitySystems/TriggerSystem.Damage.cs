using Content.Server.Explosion.Components;
using Content.Shared.Damage;

namespace Content.Server.Explosion.EntitySystems;

public sealed partial class TriggerSystem
{
    private void InitializeDamageReceived()
    {
        SubscribeLocalEvent<TriggerOnDamageReceivedComponent, DamageChangedEvent>(OnDamageReceived);
    }

    private void OnDamageReceived(EntityUid uid, TriggerOnDamageReceivedComponent component, DamageChangedEvent args)
    {
        if (args.DamageDelta == null || args.DamageDelta.Empty || !args.DamageIncreased)
            return;

        Trigger(uid);
    }
}
