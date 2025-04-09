using Content.Shared._CP14.MagicAttuning;
using Content.Shared._CP14.MagicSpellStorage.Components;
using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.Hands;

namespace Content.Shared._CP14.MagicSpellStorage;

public sealed partial class CP14SpellStorageSystem
{
    private void InitializeAccess()
    {
        SubscribeLocalEvent<CP14SpellStorageAccessHoldingComponent, GotEquippedHandEvent>(OnEquippedHand);
        SubscribeLocalEvent<CP14SpellStorageAccessHoldingComponent, AddedAttuneToMindEvent>(OnHandAddedAttune);

        SubscribeLocalEvent<CP14SpellStorageAccessWearingComponent, AddedAttuneToMindEvent>(OnClothingAddedAttune);
        SubscribeLocalEvent<CP14SpellStorageAccessWearingComponent, ClothingGotEquippedEvent>(OnClothingEquipped);
        SubscribeLocalEvent<CP14SpellStorageAccessWearingComponent, ClothingGotUnequippedEvent>(OnClothingUnequipped);
    }

    private void OnEquippedHand(Entity<CP14SpellStorageAccessHoldingComponent> ent, ref GotEquippedHandEvent args)
    {
        if (!TryComp<CP14SpellStorageComponent>(ent, out var spellStorage))
            return;

        TryGrantAccess((ent, spellStorage), args.User);
    }

    private void OnHandAddedAttune(Entity<CP14SpellStorageAccessHoldingComponent> ent, ref AddedAttuneToMindEvent args)
    {
        if (!TryComp<CP14SpellStorageComponent>(ent, out var spellStorage))
            return;

        if (args.User is null)
            return;

        if (!_hands.IsHolding(args.User.Value, ent))
            return;

        TryGrantAccess((ent, spellStorage), args.User.Value);
    }

    private void OnClothingAddedAttune(Entity<CP14SpellStorageAccessWearingComponent> ent, ref AddedAttuneToMindEvent args)
    {
        if (!ent.Comp.Wearing)
            return;

        if (!TryComp<CP14SpellStorageComponent>(ent, out var spellStorage))
            return;

        if (args.User is null)
            return;

        if (!TryComp<ClothingComponent>(ent, out var clothing))
            return;

        if (Transform(ent).ParentUid != args.User)
            return;

        TryGrantAccess((ent, spellStorage), args.User.Value);
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
