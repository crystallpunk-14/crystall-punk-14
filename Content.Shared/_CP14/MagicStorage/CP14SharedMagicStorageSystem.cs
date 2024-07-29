using Content.Shared._CP14.MagicStorage.Components;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Mind;
using Content.Shared.Verbs;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.MagicStorage;

/// <summary>
/// This system handles the storage of spells in entities, and how players obtain them.
/// </summary>
public sealed partial class CP14SharedMagicStorageSystem : EntitySystem
{
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicStorageComponent, MapInitEvent>(OnMagicStorageInit);

        SubscribeLocalEvent<CP14MagicAttuningItemComponent, GetVerbsEvent<InteractionVerb>>(OnInteractionVerb);
        SubscribeLocalEvent<CP14MagicAttuningMindComponent, CP14MagicAttuneDoAfterEvent>(OnAttuneDoAfter);
    }

    /// <summary>
    /// When we initialize, we create action entities, and add them to this item.
    /// </summary>
    private void OnMagicStorageInit(Entity<CP14MagicStorageComponent> mStorage, ref MapInitEvent args)
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

        if (focusingMind.MaxFocus <= 0)
            return false;

        return true;
    }

    private bool TryStartAttune(EntityUid user, Entity<CP14MagicAttuningItemComponent> item)
    {
        if (!_mind.TryGetMind(user, out var mindId, out var mind))
            return false;

        if (!TryComp<CP14MagicAttuningMindComponent>(mindId, out var focusingMind))
            return false;

        if (focusingMind.MaxFocus <= 0)
            return false;

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


    }
}

[Serializable, NetSerializable]
public sealed partial class CP14MagicAttuneDoAfterEvent : SimpleDoAfterEvent
{
}
