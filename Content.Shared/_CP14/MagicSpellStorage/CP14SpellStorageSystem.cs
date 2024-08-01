using Content.Shared._CP14.MagicAttuning;
using Content.Shared.Actions;
using Content.Shared.Clothing;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Mind;

namespace Content.Shared._CP14.MagicSpellStorage;

/// <summary>
/// this part of the system is responsible for storing spells in items, and the methods players use to obtain them.
/// </summary>
public sealed partial class CP14SpellStorageSystem : EntitySystem
{
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly CP14SharedMagicAttuningSystem _attuning = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CP14SpellStorageComponent, MapInitEvent>(OnMagicStorageInit);

        SubscribeLocalEvent<CP14SpellStorageAccessHoldingComponent, GotEquippedHandEvent>(OnEquippedHand);
        SubscribeLocalEvent<CP14SpellStorageAccessHoldingComponent, AddedAttuneToMindEvent>(OnHandAddedAttune);

        SubscribeLocalEvent<CP14SpellStorageAccessWearingComponent, ClothingGotEquippedEvent>(OnClothingEquipped);
        SubscribeLocalEvent<CP14SpellStorageAccessWearingComponent, ClothingGotUnequippedEvent>(OnClothingUnequipped);

        SubscribeLocalEvent<CP14SpellStorageRequireAttuneComponent, RemovedAttuneFromMindEvent>(OnRemovedAttune);
    }

    /// <summary>
    /// When we initialize, we create action entities, and add them to this item.
    /// </summary>
    private void OnMagicStorageInit(Entity<CP14SpellStorageComponent> mStorage, ref MapInitEvent args)
    {
        foreach (var spell in mStorage.Comp.Spells)
        {
            var spellEnt = _actionContainer.AddAction(mStorage, spell);
            if (spellEnt is null)
                continue;

            mStorage.Comp.SpellEntities.Add(spellEnt.Value);
        }
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

    private void OnClothingEquipped(Entity<CP14SpellStorageAccessWearingComponent> ent, ref ClothingGotEquippedEvent args)
    {
        if (!TryComp<CP14SpellStorageComponent>(ent, out var spellStorage))
            return;

        TryGrantAccess((ent, spellStorage), args.Wearer);
    }

    private void OnClothingUnequipped(Entity<CP14SpellStorageAccessWearingComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        _actions.RemoveProvidedActions(args.Wearer, ent);
    }

    private bool TryGrantAccess(Entity<CP14SpellStorageComponent> storage, EntityUid user)
    {
        if (!_mind.TryGetMind(user, out var mindId, out var mind))
            return false;

        if (mind.OwnedEntity is null)
            return false;

        if (TryComp<CP14SpellStorageRequireAttuneComponent>(storage, out var reqAttune))
        {
            if (!_attuning.IsAttunedTo(mindId, storage))
                return false;
        }

        _actions.GrantActions(user, storage.Comp.SpellEntities, storage);
        return true;
    }

    private void OnRemovedAttune(Entity<CP14SpellStorageRequireAttuneComponent> ent, ref RemovedAttuneFromMindEvent args)
    {
        if (args.User is null)
            return;

        _actions.RemoveProvidedActions(args.User.Value, ent);
    }
}
