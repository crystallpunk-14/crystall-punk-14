using System.Diagnostics.CodeAnalysis;
using Content.Shared._CP14.MagicEnergy;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Popups;

namespace Content.Server._CP14.MagicEnergy;

public sealed partial class CP14MagicEnergyCrystalSlotSystem : SharedCP14MagicEnergyCrystalSlotSystem
{

    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
    [Dependency] private readonly CP14MagicEnergySystem _magicEnergy = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAppearanceSystem _sharedAppearanceSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CP14MagicEnergyCrystalSlotComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<CP14MagicEnergyCrystalSlotComponent> ent, ref ExaminedEvent args)
    {
        if (!TryGetEnergyCrystalFromSlot(ent, out var crystalUid, out var crystalComp, ent.Comp))
            return;

        var scanEvent = new CP14MagicEnergyScanEvent();
        RaiseLocalEvent(args.Examiner, scanEvent);

        if (!scanEvent.CanScan)
            return;

        args.PushMarkup(_magicEnergy.GetEnergyExaminedText(crystalUid.Value, crystalComp));
    }

    public bool TryGetEnergyCrystalFromSlot(EntityUid uid,
        [NotNullWhen(true)] out CP14MagicEnergyContainerComponent? energyComp,
        CP14MagicEnergyCrystalSlotComponent? component = null)
    {
        return TryGetEnergyCrystalFromSlot(uid, out _, out energyComp, component);
    }

    public bool TryGetEnergyCrystalFromSlot(EntityUid uid,
        [NotNullWhen(true)] out EntityUid? energyEnt,
        [NotNullWhen(true)] out CP14MagicEnergyContainerComponent? energyComp,
        CP14MagicEnergyCrystalSlotComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
        {
            energyEnt = null;
            energyComp = null;
            return false;
        }

        if (_itemSlotsSystem.TryGetSlot(uid, component.SlotId, out ItemSlot? slot))
        {
            energyEnt = slot.Item;
            return TryComp(slot.Item, out energyComp);
        }

        energyEnt = null;
        energyComp = null;
        return false;
    }

    public bool TryUseCharge(EntityUid uid,
        FixedPoint2 energy,
        CP14MagicEnergyCrystalSlotComponent? component = null,
        EntityUid? user = null,
        bool safe = false)
    {
        if (!TryGetEnergyCrystalFromSlot(uid, out var energyEnt, out var energyComp, component))
        {
            if (user != null)
                _popup.PopupEntity(Loc.GetString("cp14-magic-energy-no-crystal"), uid,user.Value);

            return false;
        }

        if (_magicEnergy.TryConsumeEnergy(energyEnt.Value, energy, energyComp, safe))
        {
            if (user != null)
                _popup.PopupEntity(
                    Loc.GetString(safe ? "cp14-magic-energy-insufficient" : "cp14-magic-energy-insufficient-unsafe"),
                    uid,
                    user.Value);

            return false;
        }

        _sharedAppearanceSystem.SetData(uid, CP14MagicEnergyVisuals.ChargeLevel, energyComp.Energy > 0);
        return true;
    }
}
