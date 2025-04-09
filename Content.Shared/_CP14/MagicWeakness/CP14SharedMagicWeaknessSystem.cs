using Content.Shared._CP14.MagicEnergy;
using Content.Shared.Bed.Sleep;
using Content.Shared.Damage;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;

namespace Content.Shared._CP14.MagicWeakness;

public abstract class CP14SharedMagicWeaknessSystem : EntitySystem
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
        SubscribeLocalEvent<CP14MagicUnsafeDamageComponent, CP14MagicEnergyOverloadEvent>(OnMagicEnergyOverloadDamage);

        SubscribeLocalEvent<CP14MagicUnsafeSleepComponent, CP14MagicEnergyBurnOutEvent>(OnMagicEnergyBurnOutSleep);
        SubscribeLocalEvent<CP14MagicUnsafeSleepComponent, CP14MagicEnergyOverloadEvent>(OnMagicEnergyOverloadSleep);
    }

    private void OnMagicEnergyBurnOutSleep(Entity<CP14MagicUnsafeSleepComponent> ent,
        ref CP14MagicEnergyBurnOutEvent args)
    {
        if (args.BurnOutEnergy > ent.Comp.SleepThreshold)
        {
            _popup.PopupEntity(Loc.GetString("cp14-magic-energy-damage-burn-out-fall"),
                ent,
                ent,
                PopupType.LargeCaution);
            _statusEffects.TryAddStatusEffect<ForcedSleepingComponent>(ent,
                StatusEffectKey,
                TimeSpan.FromSeconds(ent.Comp.SleepPerEnergy * (float)args.BurnOutEnergy),
                false);
        }
    }

    private void OnMagicEnergyOverloadSleep(Entity<CP14MagicUnsafeSleepComponent> ent,
        ref CP14MagicEnergyOverloadEvent args)
    {
        if (args.OverloadEnergy > ent.Comp.SleepThreshold)
        {
            _popup.PopupEntity(Loc.GetString("cp14-magic-energy-damage-burn-out-fall"),
                ent,
                ent,
                PopupType.LargeCaution);
            _statusEffects.TryAddStatusEffect<ForcedSleepingComponent>(ent,
                StatusEffectKey,
                TimeSpan.FromSeconds(ent.Comp.SleepPerEnergy * (float)args.OverloadEnergy),
                false);
        }
    }

    private void OnMagicEnergyBurnOutDamage(Entity<CP14MagicUnsafeDamageComponent> ent,
        ref CP14MagicEnergyBurnOutEvent args)
    {
        //TODO: Idk why this dont popup recipient
        //Others popup
        _popup.PopupPredicted(Loc.GetString("cp14-magic-energy-damage-burn-out"),
            Loc.GetString("cp14-magic-energy-damage-burn-out-other", ("name", Identity.Name(ent, EntityManager))),
            ent,
            ent);

        //Local self popup
        _popup.PopupEntity(
            Loc.GetString("cp14-magic-energy-damage-burn-out"),
            ent,
            ent,
            PopupType.LargeCaution);

        _damageable.TryChangeDamage(ent, ent.Comp.DamagePerEnergy * args.BurnOutEnergy, interruptsDoAfters: false);
    }

    private void OnMagicEnergyOverloadDamage(Entity<CP14MagicUnsafeDamageComponent> ent,
        ref CP14MagicEnergyOverloadEvent args)
    {
        //TODO: Idk why this dont popup recipient
        //Others popup
        _popup.PopupPredicted(Loc.GetString("cp14-magic-energy-damage-overload"),
            Loc.GetString("cp14-magic-energy-damage-overload-other", ("name", Identity.Name(ent, EntityManager))),
            ent,
            ent);

        //Local self popup
        _popup.PopupEntity(
            Loc.GetString("cp14-magic-energy-damage-overload"),
            ent,
            ent,
            PopupType.LargeCaution);

        _damageable.TryChangeDamage(ent, ent.Comp.DamagePerEnergy * args.OverloadEnergy, interruptsDoAfters: false);
    }
}
