using Content.Shared._CP14.MagicStorage.Components;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Foldable;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.MagicStorage;

/// <summary>
/// This system handles the storage of spells in entities, and how players obtain them.
/// </summary>
public sealed partial class CP14SharedMagicAttuningSystem : EntitySystem
{
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicSpellStorageComponent, MapInitEvent>(OnMagicStorageInit);

        SubscribeLocalEvent<CP14MagicAttuningItemComponent, GetVerbsEvent<InteractionVerb>>(OnInteractionVerb);
        SubscribeLocalEvent<CP14MagicAttuningMindComponent, CP14MagicAttuneDoAfterEvent>(OnAttuneDoAfter);
    }

    /// <summary>
    /// When we initialize, we create action entities, and add them to this item.
    /// </summary>
    private void OnMagicStorageInit(Entity<CP14MagicSpellStorageComponent> mStorage, ref MapInitEvent args)
    {
        foreach (var spell in mStorage.Comp.Spells)
        {
            var spellEnt = _actionContainer.AddAction(mStorage, spell);
            if (spellEnt is null)
                continue;

            mStorage.Comp.SpellEntities.Add(spellEnt.Value);
        }
    }

    private void OnInteractionVerb(Entity<CP14MagicAttuningItemComponent> attuningItem, ref GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (!CanAttune(args.User, attuningItem))
            return;

        var user = args.User;
        args.Verbs.Add(new()
        {
            Act = () =>
            {
                TryStartAttune(user, attuningItem);
            },
            Text = Loc.GetString("cp14-magic-attuning-verb-text", ("item", MetaData(attuningItem).EntityName)),
            Message = Loc.GetString("cp14-magic-attuning-verb-message"),
        });
    }

    public bool CanAttune(EntityUid user, EntityUid item)
    {
        if (!_mind.TryGetMind(user, out var mindId, out var mind))
            return false;

        if (!TryComp<CP14MagicAttuningMindComponent>(mindId, out var focusingMind))
            return false;

        if (focusingMind.MaxAttuning <= 0)
            return false;

        return true;
    }

    private bool TryStartAttune(EntityUid user, Entity<CP14MagicAttuningItemComponent> item)
    {
        if (!_mind.TryGetMind(user, out var mindId, out var mind))
            return false;

        if (!TryComp<CP14MagicAttuningMindComponent>(mindId, out var attuningMind))
            return false;

        if (attuningMind.MaxAttuning <= 0)
            return false;

        //if there's an overabundance of ties, we report that the oldest one is torn.
        if (attuningMind.AttunedTo.Count > 0)
        {
            var oldestAttune = attuningMind.AttunedTo[0];
            _popup.PopupClient(Loc.GetString("cp14-magic-attune-oldest-forgot", ("item", MetaData(oldestAttune).EntityName)), user);
        }

        //we notify the current owner of the item that someone is cutting ties.
        if (item.Comp.Link is not null &&
            item.Comp.Link.Value.Owner != mindId &&
            TryComp<MindComponent>(item.Comp.Link.Value.Owner, out var ownerMind) &&
            ownerMind.OwnedEntity is not null)
        {
            _popup.PopupEntity(Loc.GetString("cp14-magic-attune-oldest-forgot", ("item", MetaData(item).EntityName)), ownerMind.OwnedEntity.Value);
        }

        var doAfterArgs = new DoAfterArgs(EntityManager,
            user,
            item.Comp.FocusTime,
            new CP14MagicAttuneDoAfterEvent(),
            mindId,
            item)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            DistanceThreshold = 2f,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
        return true;
    }

    private void OnAttuneDoAfter(Entity<CP14MagicAttuningMindComponent> ent, ref CP14MagicAttuneDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (ent.Comp.AttunedTo.Count >= ent.Comp.MaxAttuning)
        {
            var oldestAttune = ent.Comp.AttunedTo[0];
            RemoveAttune(ent, oldestAttune);
        }
    }

    private void RemoveAttune(Entity<CP14MagicAttuningMindComponent> attuningMind, EntityUid item)
    {
        if (!attuningMind.Comp.AttunedTo.Contains(item))
            return;

        attuningMind.Comp.AttunedTo.Remove(item);

        if (!TryComp<CP14MagicAttuningItemComponent>(item, out var attuningItem))
            return;

        attuningItem.Link = null;

        var ev = new RemovedAttuneFromMindEvent(attuningMind, item);
        RaiseLocalEvent(attuningMind, ev);
        RaiseLocalEvent(item, ev);

        if (TryComp<MindComponent>(attuningMind, out var mind))
        {
            _popup.PopupClient(Loc.GetString("cp14-magic-attune-oldest-forgot-end", ("item", MetaData(item).EntityName)), mind.OwnedEntity);
        }
    }

    private void AddAttune(Entity<CP14MagicAttuningMindComponent> attuningMind, EntityUid item)
    {
        if (!attuningMind.Comp.AttunedTo.Contains(item))
            return;

        if (!TryComp<CP14MagicAttuningItemComponent>(item, out var attuningItem))
            return;

        if (attuningItem.Link is not null)
            RemoveAttune(attuningItem.Link.Value, item);

        attuningMind.Comp.AttunedTo.Add(item);
        attuningItem.Link = attuningMind;
    }
}

[Serializable, NetSerializable]
public sealed partial class CP14MagicAttuneDoAfterEvent : SimpleDoAfterEvent
{
}

/// <summary>
/// is evoked on both the item and the mind when a new connection between them appears.
/// </summary>
public sealed class AddedAttuneToMindEvent : EntityEventArgs
{
    public readonly EntityUid Mind;
    public readonly EntityUid Item;

    public AddedAttuneToMindEvent(EntityUid mind, EntityUid item)
    {
        Mind = mind;
        Item = item;
    }
}
/// <summary>
/// is evoked on both the item and the mind when the connection is broken
/// </summary>
public sealed class RemovedAttuneFromMindEvent : EntityEventArgs
{
    public readonly EntityUid Mind;
    public readonly EntityUid Item;

    public RemovedAttuneFromMindEvent(EntityUid mind, EntityUid item)
    {
        Mind = mind;
        Item = item;
    }
}
