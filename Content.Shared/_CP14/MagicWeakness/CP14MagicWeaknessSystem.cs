using Content.Shared._CP14.MagicEnergy;
using Content.Shared.Bed.Sleep;
using Content.Shared.Damage;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;

namespace Content.Shared._CP14.MagicWeakness;

public partial class CP14MagicWeaknessSystem : EntitySystem
{
    [ValidatePrototypeId<StatusEffectPrototype>]
    private const string StatusEffectKey = "ForcedSleep";

    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicUnsafeDamageComponent, CP14MagicEnergyBurnOutEvent>(OnMagicEnergyBurnOutDamage);
        SubscribeLocalEvent<CP14MagicUnsafeSleepComponent, CP14MagicEnergyBurnOutEvent>(OnMagicEnergyBurnOutSleep);
    }

    private void OnMagicEnergyBurnOutSleep(Entity<CP14MagicUnsafeSleepComponent> ent, ref CP14MagicEnergyBurnOutEvent args)
    {
        if (args.BurnOutEnergy > ent.Comp.SleepThreshold)
        {
            _popup.PopupEntity(Loc.GetString("cp14-magic-energy-damage-burn-out-fall"), ent, ent, PopupType.LargeCaution);
            _statusEffects.TryAddStatusEffect<ForcedSleepingComponent>(ent,
                StatusEffectKey,
                TimeSpan.FromSeconds(ent.Comp.SleepPerEnergy * (float)args.BurnOutEnergy),
                false);
        }
    }

    private void OnMagicEnergyBurnOutDamage(Entity<CP14MagicUnsafeDamageComponent> ent, ref CP14MagicEnergyBurnOutEvent args)
    {
        _popup.PopupEntity(Loc.GetString("cp14-magic-energy-damage-burn-out"), ent, ent, PopupType.LargeCaution);
        _damageable.TryChangeDamage(ent, ent.Comp.DamagePerEnergy * args.BurnOutEnergy);
    }
}
