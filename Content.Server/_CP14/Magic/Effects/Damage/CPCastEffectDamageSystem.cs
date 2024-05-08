using Content.Shared.Damage;

namespace Content.Server._CP14.Magic.Effects.Damage;

public sealed class CPCastEffectDamageSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CPCastEffectDamageComponent, CPMagicCastedEvent>(OnCasted);
    }

    private void OnCasted(Entity<CPCastEffectDamageComponent> effect, ref CPMagicCastedEvent args)
    {
        if (args.Target is null)
            return;

        _damageable.TryChangeDamage(args.Target, effect.Comp.Damage, effect.Comp.IgnoreResistances,
            effect.Comp.InterruptsDoAfters);
    }
}
