using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Containers;

namespace Content.Shared._CP14.MagicEnergy;

public partial class SharedCP14MagicEnergyCrystalSlotSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicEnergyCrystalSlotComponent, EntInsertedIntoContainerMessage>(OnCrystalInserted);
        SubscribeLocalEvent<CP14MagicEnergyCrystalSlotComponent, EntRemovedFromContainerMessage>(OnCrystalRemoved);
    }

    private void OnCrystalRemoved(Entity<CP14MagicEnergyCrystalSlotComponent> slot, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != slot.Comp.SlotId)
            return;

        _appearance.SetData(slot, CP14MagicSlotVisuals.Inserted, false);
        _appearance.SetData(slot, CP14MagicSlotVisuals.Powered, false);
        RaiseLocalEvent(slot, new CP14SlotCrystalChangedEvent(true));
    }

    private void OnCrystalInserted(Entity<CP14MagicEnergyCrystalSlotComponent> slot, ref EntInsertedIntoContainerMessage args)
    {
        if (!slot.Comp.Initialized)
            return;

        if (args.Container.ID != slot.Comp.SlotId)
            return;

        _appearance.SetData(slot, CP14MagicSlotVisuals.Inserted, true);
        RaiseLocalEvent(slot, new CP14SlotCrystalChangedEvent(false));
    }
}
