
using Content.Shared.Damage;
using Content.Shared.Inventory;

namespace Content.Shared._CP14.Armor;

public sealed class CP14ArmorSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14ArmorDamageAbsorptionComponent, InventoryRelayedEvent<DamageChangedEvent>>(OnDamageChanged);
    }

    private void OnDamageChanged(Entity<CP14ArmorDamageAbsorptionComponent> ent, ref InventoryRelayedEvent<DamageChangedEvent> args)
    {
        if (!args.Args.DamageIncreased)
            return;

        if (args.Args.DamageDelta is null)
            return;

        _damageable.TryChangeDamage(ent, args.Args.DamageDelta * ent.Comp.Absorption);
    }
}
