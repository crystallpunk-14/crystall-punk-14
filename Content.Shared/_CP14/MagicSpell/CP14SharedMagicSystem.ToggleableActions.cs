using Content.Shared._CP14.MagicSpell.Components;
using Content.Shared._CP14.MagicSpell.Events;
using Content.Shared._CP14.MagicSpell.Spells;
using Content.Shared.DoAfter;

namespace Content.Shared._CP14.MagicSpell;

public abstract partial class CP14SharedMagicSystem
{
    private void InitializeToggleableActions()
    {
        SubscribeLocalEvent<CP14ToggleableInstantActionEvent>(OnInstantAction);

        SubscribeLocalEvent<CP14MagicEffectComponent, CP14ToggleableInstantActionDoAfterEvent>(OnToggleableInstantActionDoAfterEvent);
    }

    private void UpdateToggleableActions()
    {
        var query = EntityQueryEnumerator<CP14MagicEffectComponent, CP14MagicEffectToggledComponent>();

        while (query.MoveNext(out var uid, out var effect, out var toggled))
        {
            if (toggled.NextTick > _timing.CurTime)
                continue;

            if (toggled.Performer is null)
                continue;

            toggled.NextTick = _timing.CurTime + TimeSpan.FromSeconds(toggled.Frequency);

            var spellArgs =
                new CP14SpellEffectBaseArgs(toggled.Performer, effect.SpellStorage, toggled.Performer, Transform(toggled.Performer.Value).Coordinates);

            if (!CanCastSpell((uid, effect), toggled.Performer.Value))
            {
                _doAfter.Cancel(toggled.DoAfterId);
                continue;
            }

            CastSpell((uid, effect), spellArgs);
        }
    }

    private void OnToggleableInstantActionDoAfterEvent(Entity<CP14MagicEffectComponent> ent, ref CP14ToggleableInstantActionDoAfterEvent args)
    {
        if (!TryComp<CP14MagicEffectToggledComponent>(ent, out var toggled))
            return;

        _action.CP14StartCustomDelay(ent, TimeSpan.FromSeconds(toggled.Cooldown));
        RemCompDeferred<CP14MagicEffectToggledComponent>(ent);

        var endEv = new CP14EndCastMagicEffectEvent(args.User);
        RaiseLocalEvent(ent, ref endEv);
    }

    private void StartToggleableAction(ICP14ToggleableMagicEffect toggleable, DoAfterEvent doAfter, Entity<CP14MagicEffectComponent> action, EntityUid performer)
    {
        var fromItem = action.Comp.SpellStorage is not null;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, performer, toggleable.CastTime, doAfter, action, used: action.Comp.SpellStorage)
        {
            BreakOnMove = toggleable.BreakOnMove,
            BreakOnDamage = toggleable.BreakOnDamage,
            Hidden = toggleable.Hidden,
            DistanceThreshold = 100f,
            CancelDuplicate = true,
            BlockDuplicate = true,
            BreakOnDropItem = fromItem,
            NeedHand = fromItem,
        };

        _doAfter.TryStartDoAfter(doAfterEventArgs, out var doAfterId);

        EnsureComp<CP14MagicEffectToggledComponent>(action, out var toggled);
        toggled.Frequency = toggleable.EffectFrequency;
        toggled.Performer = performer;
        toggled.DoAfterId = doAfterId;
        toggled.Cooldown = toggleable.Cooldown;
    }

    /// <summary>
    /// Instant action used from hotkey event
    /// </summary>
    private void OnInstantAction(CP14ToggleableInstantActionEvent args)
    {
        if (args.Handled)
            return;

        if (args is not ICP14ToggleableMagicEffect toggleable)
            return;

        if (!TryComp<CP14MagicEffectComponent>(args.Action, out var magicEffect))
            return;

        Entity<CP14MagicEffectComponent> spell = (args.Action, magicEffect);

        if (!CanCastSpell(spell, args.Performer))
            return;

        var doAfter = new CP14ToggleableInstantActionDoAfterEvent(args.Cooldown);

        StartToggleableAction(toggleable, doAfter, (args.Action, magicEffect), args.Performer);

        var evStart = new CP14StartCastMagicEffectEvent(args.Performer);
        RaiseLocalEvent(args.Action, ref evStart);

        args.Handled = true;
    }
}

