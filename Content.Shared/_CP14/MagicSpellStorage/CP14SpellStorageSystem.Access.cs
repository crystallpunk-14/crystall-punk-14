using Content.Shared._CP14.MagicSpellStorage.Components;
using Content.Shared.Clothing;
using Content.Shared.Hands;

namespace Content.Shared._CP14.MagicSpellStorage;

public sealed partial class CP14SpellStorageSystem
{
    private void InitializeAccess()
    {
        SubscribeLocalEvent<CP14SpellStorageAccessHoldingComponent, GotEquippedHandEvent>(OnEquippedHand);

        SubscribeLocalEvent<CP14SpellStorageAccessWearingComponent, ClothingGotEquippedEvent>(OnClothingEquipped);
        SubscribeLocalEvent<CP14SpellStorageAccessWearingComponent, ClothingGotUnequippedEvent>(OnClothingUnequipped);
    }

    private void OnEquippedHand(Entity<CP14SpellStorageAccessHoldingComponent> ent, ref GotEquippedHandEvent args)
    {
        if (!TryComp<CP14SpellStorageComponent>(ent, out var spellStorage))
            return;

        TryGrantAccess((ent, spellStorage), args.User);
    }

    private void OnClothingEquipped(Entity<CP14SpellStorageAccessWearingComponent> ent, ref ClothingGotEquippedEvent args)
    {
        ent.Comp.Wearing = true;

        if (!TryComp<CP14SpellStorageComponent>(ent, out var spellStorage))
            return;

        TryGrantAccess((ent, spellStorage), args.Wearer);
    }

    private void OnClothingUnequipped(Entity<CP14SpellStorageAccessWearingComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        ent.Comp.Wearing = false;

        _actions.RemoveProvidedActions(args.Wearer, ent);
    }
}
