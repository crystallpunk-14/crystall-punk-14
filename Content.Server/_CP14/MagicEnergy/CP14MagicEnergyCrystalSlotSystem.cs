using System.Diagnostics.CodeAnalysis;
using Content.Shared._CP14.MagicEnergy;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Popups;
using Robust.Shared.Containers;

namespace Content.Server._CP14.MagicEnergy;

public sealed partial class CP14MagicEnergyCrystalSlotSystem : SharedCP14MagicEnergyCrystalSlotSystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly CP14MagicEnergySystem _magicEnergy = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CP14MagicEnergyCrystalComponent, CP14MagicEnergyLevelChangeEvent>(OnEnergyChanged);
        SubscribeLocalEvent<CP14MagicEnergyCrystalSlotComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<CP14MagicEnergyCrystalSlotComponent, CP14SlotCrystalChangedEvent>(OnCrystalChanged);
    }

    private void OnCrystalChanged(Entity<CP14MagicEnergyCrystalSlotComponent> ent, ref CP14SlotCrystalChangedEvent args)
    {
        var realPowered = TryGetEnergyCrystalFromSlot(ent, out var energyEnt, out var energyComp);

        if (energyComp != null)
            realPowered = energyComp.Energy > 0;

        if (ent.Comp.Powered != realPowered)
        {
            ent.Comp.Powered = realPowered;
            _appearance.SetData(ent, CP14MagicSlotVisuals.Powered, realPowered);
            RaiseLocalEvent(ent, new CP14SlotCrystalPowerChangedEvent(realPowered));
        }
    }

    private void OnEnergyChanged(Entity<CP14MagicEnergyCrystalComponent> crystal, ref CP14MagicEnergyLevelChangeEvent args)
    {
        if (_container.TryGetContainingContainer(crystal, out var container)
            && TryComp(container.Owner, out CP14MagicEnergyCrystalSlotComponent? slot)
            && _itemSlots.TryGetSlot(container.Owner, slot.SlotId, out var itemSlot))
        {
            if (itemSlot.Item == crystal)
            {
                RaiseLocalEvent(container.Owner, new CP14SlotCrystalChangedEvent(false));
            }
        }
    }

    private void OnExamined(Entity<CP14MagicEnergyCrystalSlotComponent> ent, ref ExaminedEvent args)
    {
        if (!TryGetEnergyCrystalFromSlot(ent, out var crystalUid, out var crystalComp, ent.Comp))
            return;

        if (!args.IsInDetailsRange)
            return;

        //var scanEvent = new CP14MagicEnergyScanEvent();
        //RaiseLocalEvent(args.Examiner, scanEvent);
//
        //if (!scanEvent.CanScan)
        //    return;

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

        if (_itemSlots.TryGetSlot(uid, component.SlotId, out ItemSlot? slot))
        {
            energyEnt = slot.Item;
            return TryComp(slot.Item, out energyComp);
        }

        energyEnt = null;
        energyComp = null;
        return false;
    }

    public bool HasEnergy(EntityUid uid,
        FixedPoint2 energy,
        CP14MagicEnergyCrystalSlotComponent? component = null,
        EntityUid? user = null)
    {
        if (!TryGetEnergyCrystalFromSlot(uid, out var energyEnt, out var energyComp, component))
        {
            if (user != null)
                _popup.PopupEntity(Loc.GetString("cp14-magic-energy-no-crystal"), uid,user.Value);

            return false;
        }

        if (energyComp.Energy < energy)
        {
            if (user != null)
                _popup.PopupEntity(Loc.GetString("cp14-magic-energy-insufficient"), uid, user.Value);

            return false;
        }

        return true;
    }

    public bool TryChangeEnergy(EntityUid uid,
        FixedPoint2 energy,
        CP14MagicEnergyCrystalSlotComponent? component = null,
        EntityUid? user = null,
        bool safe = false)
    {
        if (!TryGetEnergyCrystalFromSlot(uid, out var energyEnt, out var energyComp, component))
        {
            if (user != null)
                _popup.PopupEntity(Loc.GetString("cp14-magic-energy-no-crystal"), uid, user.Value);

            return false;
        }

        _magicEnergy.ChangeEnergy(energyEnt.Value, energy, out _, out _, energyComp, safe);
        return true;
    }
}
