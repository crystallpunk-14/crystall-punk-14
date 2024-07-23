using Content.Server._CP14.MeleeWeapon.Components;
using Content.Shared.Damage;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server._CP14.MeleeWeapon.EntitySystems;

public sealed class CP14MeleeSelfDamageSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CP14MeleeSelfDamageComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<CP14MeleeSelfDamageComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;
        if (args.HitEntities.Count == 0)
            return;
        _damageable.TryChangeDamage(ent, ent.Comp.DamageToSelf);
    }
}
