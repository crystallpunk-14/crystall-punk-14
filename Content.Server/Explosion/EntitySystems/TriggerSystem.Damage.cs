using Content.Server.Explosion.Components;
using Content.Shared.Damage;
using Robust.Shared.GameObjects;

namespace Content.Server.Explosion.EntitySystems;

public sealed partial class TriggerSystem
{
    private void InitializeDamage()
    {
        SubscribeLocalEvent<TriggerOnDamageComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void OnDamageChanged(EntityUid uid, TriggerOnDamageComponent component, DamageChangedEvent args)
    {
        if (args.DamageDelta == null || args.DamageDelta.Empty)
            return;

        Trigger(uid);
    }
}
