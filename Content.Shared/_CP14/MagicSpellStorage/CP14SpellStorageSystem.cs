using Content.Shared._CP14.MagicAttuning;
using Content.Shared._CP14.MagicSpell.Components;
using Content.Shared._CP14.MagicSpell.Events;
using Content.Shared._CP14.MagicSpellStorage.Components;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Mind;
using Robust.Shared.Network;

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
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        InitializeAccess();

        SubscribeLocalEvent<CP14SpellStorageComponent, MapInitEvent>(OnMagicStorageInit);
        SubscribeLocalEvent<CP14SpellStorageComponent, ComponentShutdown>(OnMagicStorageShutdown);

        SubscribeLocalEvent<CP14SpellStorageUseDamageComponent, CP14SpellFromSpellStorageUsedEvent>(OnSpellUsed);
        SubscribeLocalEvent<CP14SpellStorageRequireAttuneComponent, RemovedAttuneFromMindEvent>(OnRemovedAttune);
    }

    private void OnSpellUsed(Entity<CP14SpellStorageUseDamageComponent> ent, ref CP14SpellFromSpellStorageUsedEvent args)
    {
        _damageable.TryChangeDamage(ent, ent.Comp.DamagePerMana * args.Manacost);
    }

    /// <summary>
    /// When we initialize, we create action entities, and add them to this item.
    /// </summary>
    private void OnMagicStorageInit(Entity<CP14SpellStorageComponent> storage, ref MapInitEvent args)
    {
        if (_net.IsClient)
            return;

        foreach (var spell in storage.Comp.Spells)
        {
            var spellEnt = _actionContainer.AddAction(storage, spell);
            if (spellEnt is null)
                continue;

            var provided = EntityManager.EnsureComponent<CP14MagicEffectComponent>(spellEnt.Value);
            provided.SpellStorage = storage;

            storage.Comp.SpellEntities.Add(spellEnt.Value);
        }
        if (storage.Comp.GrantAccessToSelf)
            _actions.GrantActions(storage, storage.Comp.SpellEntities, storage);
    }

    private void OnMagicStorageShutdown(Entity<CP14SpellStorageComponent> mStorage, ref ComponentShutdown args)
    {
        if (_net.IsClient)
            return;

        foreach (var spell in mStorage.Comp.SpellEntities)
        {
            QueueDel(spell);
        }
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
