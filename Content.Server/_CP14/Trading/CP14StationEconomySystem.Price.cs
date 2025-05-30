using Content.Server.Cargo.Systems;
using Content.Shared.Weapons.Melee;

namespace Content.Server._CP14.Trading;

public sealed partial class CP14StationEconomySystem
{
    private void InitPriceEvents()
    {
        SubscribeLocalEvent<MeleeWeaponComponent, PriceCalculationEvent>(OnMeleeWeaponPriceCalculation);
    }

    private void OnMeleeWeaponPriceCalculation(Entity<MeleeWeaponComponent> ent, ref PriceCalculationEvent args)
    {
        //double price = 0;
        //var dps = ent.Comp.Damage.GetTotal() * ent.Comp.AttackRate;
        //if (dps <= 0)
        //    return;
//
        //price += dps.Value;
//
        //if (ent.Comp.ResetOnHandSelected == false)
        //    price *= 1.5; // If the weapon doesn't reset on hand selection, it's more valuable.
//
        //if (ent.Comp.AltDisarm)
        //    price *= 1.5; // If the weapon has an alt disarm, it's more valuable.
//
        //args.Price += price * 0.1f;
    }
}
