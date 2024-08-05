using Content.Shared.DoAfter;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.MagicAttuning;

/// <summary>
/// This system controls the customization to magic items by the players.
/// </summary>
public sealed partial class CP14SharedMagicAttuningSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicAttuningItemComponent, GetVerbsEvent<InteractionVerb>>(OnInteractionVerb);
        SubscribeLocalEvent<CP14MagicAttuningMindComponent, CP14MagicAttuneDoAfterEvent>(OnAttuneDoAfter);
        SubscribeLocalEvent<CP14MagicAttuningMindComponent, MindAddedMessage>(OnMindAdded);
    }

    private void OnMindAdded(Entity<CP14MagicAttuningMindComponent> ent, ref MindAddedMessage args)
    {
        if (!ent.Comp.AutoCopyToMind)
            return;

        if (HasComp<MindComponent>(ent))
            return;

        if (!_mind.TryGetMind(ent, out var mindId, out var mind))
            return;

        if (!HasComp<CP14MagicAttuningMindComponent>(mindId))
        {
            var attuneMind = AddComp<CP14MagicAttuningMindComponent>(mindId);
            attuneMind.MaxAttuning = ent.Comp.MaxAttuning;
        }
    }

    public bool IsAttunedTo(EntityUid mind, EntityUid item)
    {
        if (!TryComp<CP14MagicAttuningItemComponent>(item, out var attuningItem))
            return false;

        if (!TryComp<CP14MagicAttuningMindComponent>(mind, out var attuningMind))
            return false;

        return attuningMind.AttunedTo.Contains(item);
    }

    private void OnInteractionVerb(Entity<CP14MagicAttuningItemComponent> attuningItem, ref GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (!_mind.TryGetMind(args.User, out var mindId, out var mind))
            return;

        if (!TryComp<CP14MagicAttuningMindComponent>(mindId, out var attumingMind))
            return;

        var user = args.User;
        if (attumingMind.AttunedTo.Contains(args.Target))
        {
            args.Verbs.Add(new()
            {
                Act = () =>
                {
                    RemoveAttune((mindId, attumingMind), attuningItem);
                },
                Text = Loc.GetString("cp14-magic-deattuning-verb-text"),
                Message = Loc.GetString("cp14-magic-attuning-verb-message"),
            });
        }
        else
        {
            args.Verbs.Add(new()
            {
                Act = () =>
                {
                    TryStartAttune(user, attuningItem);
                },
                Text = Loc.GetString("cp14-magic-attuning-verb-text"),
                Message = Loc.GetString("cp14-magic-attuning-verb-message"),
            });
        }
    }

    public bool TryStartAttune(EntityUid user, Entity<CP14MagicAttuningItemComponent> item)
    {
        if (!_mind.TryGetMind(user, out var mindId, out var mind))
            return false;

        if (!TryComp<CP14MagicAttuningMindComponent>(mindId, out var attuningMind))
            return false;

        if (attuningMind.MaxAttuning <= 0)
            return false;

        //if there's an overabundance of ties, we report that the oldest one is torn.
        if (attuningMind.AttunedTo.Count >= attuningMind.MaxAttuning)
        {
            var oldestAttune = attuningMind.AttunedTo[0];
            _popup.PopupEntity(Loc.GetString("cp14-magic-attune-oldest-forgot", ("item", MetaData(oldestAttune).EntityName)), user, user);
        }

        //we notify the current owner of the item that someone is cutting ties.
        if (item.Comp.Link is not null &&
            item.Comp.Link.Value.Owner != mindId &&
            TryComp<MindComponent>(item.Comp.Link.Value.Owner, out var ownerMind) &&
            ownerMind.OwnedEntity is not null)
        {
            _popup.PopupEntity(Loc.GetString("cp14-magic-attune-oldest-forgot", ("item", MetaData(item).EntityName)), ownerMind.OwnedEntity.Value, ownerMind.OwnedEntity.Value);
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
            BlockDuplicate = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
        return true;
    }

    private void OnAttuneDoAfter(Entity<CP14MagicAttuningMindComponent> ent, ref CP14MagicAttuneDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is null)
            return;

        if (ent.Comp.AttunedTo.Count >= ent.Comp.MaxAttuning)
        {
            var oldestAttune = ent.Comp.AttunedTo[0];
            RemoveAttune(ent, oldestAttune);
        }

        AddAttune(ent, args.Target.Value);
    }

    private void RemoveAttune(Entity<CP14MagicAttuningMindComponent> attuningMind, EntityUid item)
    {
        if (!attuningMind.Comp.AttunedTo.Contains(item))
            return;

        attuningMind.Comp.AttunedTo.Remove(item);

        if (!TryComp<CP14MagicAttuningItemComponent>(item, out var attuningItem))
            return;

        if (!TryComp<MindComponent>(attuningMind, out var mind))
            return;

        attuningItem.Link = null;

        var ev = new RemovedAttuneFromMindEvent(attuningMind, mind.OwnedEntity, item);
        RaiseLocalEvent(attuningMind, ev);
        RaiseLocalEvent(item, ev);

        if (mind.OwnedEntity is not null)
        {
            _popup.PopupEntity(Loc.GetString("cp14-magic-attune-oldest-forgot-end", ("item", MetaData(item).EntityName)), mind.OwnedEntity.Value, mind.OwnedEntity.Value);
        }
    }

    private void AddAttune(Entity<CP14MagicAttuningMindComponent> attuningMind, EntityUid item)
    {
        if (attuningMind.Comp.AttunedTo.Contains(item))
            return;

        if (!TryComp<CP14MagicAttuningItemComponent>(item, out var attuningItem))
            return;

        if (!TryComp<MindComponent>(attuningMind, out var mind))
            return;

        if (attuningItem.Link is not null)
            RemoveAttune(attuningItem.Link.Value, item);

        attuningMind.Comp.AttunedTo.Add(item);
        attuningItem.Link = attuningMind;


        var ev = new AddedAttuneToMindEvent(attuningMind, mind.OwnedEntity, item);
        RaiseLocalEvent(attuningMind, ev);
        RaiseLocalEvent(item, ev);
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
    public readonly EntityUid? User;
    public readonly EntityUid Item;

    public AddedAttuneToMindEvent(EntityUid mind, EntityUid? user, EntityUid item)
    {
        Mind = mind;
        User = user;
        Item = item;
    }
}
/// <summary>
/// is evoked on both the item and the mind when the connection is broken
/// </summary>
public sealed class RemovedAttuneFromMindEvent : EntityEventArgs
{
    public readonly EntityUid Mind;
    public readonly EntityUid? User;
    public readonly EntityUid Item;

    public RemovedAttuneFromMindEvent(EntityUid mind, EntityUid? user, EntityUid item)
    {
        Mind = mind;
        User = user;
        Item = item;
    }
}
