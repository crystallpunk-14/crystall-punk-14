using Content.Server.Explosion.Components;
using Content.Shared.Damage;

namespace Content.Server.Explosion.EntitySystems;

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
