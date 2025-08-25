using Content.Shared._CP14.Explosion.Components;
using Content.Shared.Damage;

namespace Content.Shared.Trigger.Systems;

public sealed partial class TriggerSystem
{
    private void InitializeDamageReceived()
    {
        SubscribeLocalEvent<CP14TriggerOnDamageReceivedComponent, DamageChangedEvent>(OnDamageReceived);
    }

    private void OnDamageReceived(EntityUid uid, CP14TriggerOnDamageReceivedComponent component, DamageChangedEvent args)
    {
        if (!args.DamageIncreased)
            return;

        Trigger(uid);
    }
}
