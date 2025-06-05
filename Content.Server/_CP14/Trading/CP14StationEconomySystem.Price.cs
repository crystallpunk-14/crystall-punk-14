using Content.Server.Cargo.Systems;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.Weapons.Melee;

namespace Content.Server._CP14.Trading;

public sealed partial class CP14StationEconomySystem
{
    private void InitPriceEvents()
    {
        SubscribeLocalEvent<MeleeWeaponComponent, PriceCalculationEvent>(OnMeleeWeaponPriceCalculation);
        SubscribeLocalEvent<CP14MagicEnergyContainerComponent, PriceCalculationEvent>(OnMagicEnergyPriceCalculation);
    }

    private void OnMeleeWeaponPriceCalculation(Entity<MeleeWeaponComponent> ent, ref PriceCalculationEvent args)
    {
        double price = 0;
        var dps = ent.Comp.Damage.GetTotal() * ent.Comp.AttackRate;
        if (dps <= 0)
            return;

        price += dps.Value * ent.Comp.CPWeaponPrice;

        if (ent.Comp.ResetOnHandSelected == false)
            price *= 1.5; // If the weapon doesn't reset on hand selection, it's more valuable.

        if (ent.Comp.AltDisarm)
            price *= 1.5; // If the weapon has an alt disarm, it's more valuable.

        args.Price += price * 0.1f;
    }

    private void OnMagicEnergyPriceCalculation(Entity<CP14MagicEnergyContainerComponent> ent, ref PriceCalculationEvent args)
    {
        args.Price += (double)(ent.Comp.Energy * 0.1f);
    }
}
