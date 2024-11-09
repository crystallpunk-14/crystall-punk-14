using Content.Shared._CP14.MagicSpell.Components;
using Content.Shared._CP14.MagicSpell.Events;
using Content.Shared._CP14.MagicSpell.Spells;
using Robust.Shared.Map;

namespace Content.Shared._CP14.MagicSpell;

public abstract partial class CP14SharedMagicSystem
{
    private void InitializeActions()
    {
        SubscribeLocalEvent<CP14DelayedInstantActionEvent>(OnInstantAction);
        SubscribeLocalEvent<CP14DelayedEntityWorldTargetActionEvent>(OnEntityWorldTargetAction);
        SubscribeLocalEvent<CP14DelayedEntityTargetActionEvent>(OnEntityTargetAction);

        SubscribeLocalEvent<CP14MagicEffectComponent, CP14DelayedInstantActionDoAfterEvent>(OnDelayedInstantActionDoAfter);
        SubscribeLocalEvent<CP14MagicEffectComponent, CP14DelayedEntityWorldTargetActionDoAfterEvent>(OnDelayedEntityWorldTargetDoAfter);
        SubscribeLocalEvent<CP14MagicEffectComponent, CP14DelayedEntityTargetActionDoAfterEvent>(OnDelayedEntityTargetDoAfter);
    }

    /// <summary>
    /// Instant action used from hotkey event
    /// </summary>
    private void OnInstantAction(CP14DelayedInstantActionEvent args)
    {
        if (args.Handled)
            return;

        if (args is not ICP14DelayedMagicEffect delayedEffect)
            return;

        if (!TryComp<CP14MagicEffectComponent>(args.Action, out var magicEffect))
            return;

        Entity<CP14MagicEffectComponent> spell = (args.Action, magicEffect);

        if (!CanCastSpell(spell, args.Performer))
            return;

        if (args.CastDelay > 0)
        {
            var doAfter = new CP14DelayedInstantActionDoAfterEvent(args.Cooldown);
            if (!TryCastSpellDelayed(delayedEffect, doAfter, (args.Action, magicEffect), args.Performer))
                return;
        }

        var evStart = new CP14StartCastMagicEffectEvent( args.Performer);
        RaiseLocalEvent(args.Action, ref evStart);

        var spellArgs =
            new CP14SpellEffectBaseArgs(args.Performer, args.Performer, Transform(args.Performer).Coordinates);

        CastTelegraphy((args.Action, magicEffect), spellArgs);

        if (args.CastDelay == 0)
            CastSpell((args.Action, magicEffect), spellArgs, args.Cooldown);

        args.Handled = true;
    }

    /// <summary>
    /// Target action used from hotkey event
    /// </summary>
    private void OnEntityWorldTargetAction(CP14DelayedEntityWorldTargetActionEvent args)
    {
        if (args.Handled)
            return;

        if (args is not ICP14DelayedMagicEffect delayedEffect)
            return;

        if (!TryComp<CP14MagicEffectComponent>(args.Action, out var magicEffect))
            return;

        Entity<CP14MagicEffectComponent> spell = (args.Action, magicEffect);

        if (!CanCastSpell(spell, args.Performer))
            return;


        if (args.CastDelay > 0)
        {
            var doAfter = new CP14DelayedEntityWorldTargetActionDoAfterEvent(
                EntityManager.GetNetCoordinates(args.Coords),
                EntityManager.GetNetEntity(args.Entity),
                args.Cooldown);

            if (!TryCastSpellDelayed(delayedEffect, doAfter, (args.Action, magicEffect), args.Performer))
                return;
        }

        var evStart = new CP14StartCastMagicEffectEvent( args.Performer);
        RaiseLocalEvent(args.Action, ref evStart);

        var spellArgs =
            new CP14SpellEffectBaseArgs(args.Performer, args.Entity, args.Coords);

        CastTelegraphy((args.Action, magicEffect), spellArgs);

        if (args.CastDelay <= 0)
            CastSpell((args.Action, magicEffect), spellArgs, args.Cooldown);

        args.Handled = true;
    }

    /// <summary>
    /// Target action used from hotkey event
    /// </summary>
    private void OnEntityTargetAction(CP14DelayedEntityTargetActionEvent args)
    {
        if (args.Handled)
            return;

        if (args is not ICP14DelayedMagicEffect delayedEffect)
            return;

        if (!TryComp<CP14MagicEffectComponent>(args.Action, out var magicEffect))
            return;

        Entity<CP14MagicEffectComponent> spell = (args.Action, magicEffect);

        if (!CanCastSpell(spell, args.Performer))
            return;

        if (args.CastDelay > 0)
        {
            var doAfter = new CP14DelayedEntityTargetActionDoAfterEvent(
                EntityManager.GetNetEntity(args.Target),
                args.Cooldown);

            if (!TryCastSpellDelayed(delayedEffect, doAfter, (args.Action, magicEffect), args.Performer))
                return;
        }

        var evStart = new CP14StartCastMagicEffectEvent( args.Performer);
        RaiseLocalEvent(args.Action, ref evStart);

        var spellArgs =
            new CP14SpellEffectBaseArgs(args.Performer, args.Target, Transform(args.Target).Coordinates);

        CastTelegraphy((args.Action, magicEffect), spellArgs);

        //TODO: Bug! If CastDelay = 0, cooldown dont want apply to spell aftercast
        if (args.CastDelay <= 0)
            CastSpell((args.Action, magicEffect), spellArgs, args.Cooldown);

        args.Handled = true;
    }

    /// <summary>
    /// Action doAfter finished or interrupted
    /// </summary>
    private void OnDelayedInstantActionDoAfter(Entity<CP14MagicEffectComponent> ent, ref CP14DelayedInstantActionDoAfterEvent args)
    {
        var endEv = new CP14EndCastMagicEffectEvent(args.User);
        RaiseLocalEvent(ent, ref endEv);

        if (args.Cancelled || args.Handled)
            return;

        CastSpell(ent, new CP14SpellEffectBaseArgs(args.User, args.User, Transform(args.User).Coordinates), args.Cooldown ?? 0);

        args.Handled = true;
    }

    /// <summary>
    /// Action doAfter finished or interrupted
    /// </summary>
    private void OnDelayedEntityWorldTargetDoAfter(Entity<CP14MagicEffectComponent> ent,
        ref CP14DelayedEntityWorldTargetActionDoAfterEvent args)
    {
        var endEv = new CP14EndCastMagicEffectEvent(args.User);
        RaiseLocalEvent(ent, ref endEv);

        if (args.Cancelled || args.Handled)
            return;

        var targetPos = EntityManager.GetCoordinates(args.TargetPosition);
        EntityManager.TryGetEntity(args.TargetEntity, out var targetEnt);

        CastSpell(ent, new CP14SpellEffectBaseArgs(args.User, targetEnt, targetPos), args.Cooldown ?? 0);

        args.Handled = true;
    }

    /// <summary>
    /// Action doAfter finished or interrupted
    /// </summary>
    private void OnDelayedEntityTargetDoAfter(Entity<CP14MagicEffectComponent> ent, ref CP14DelayedEntityTargetActionDoAfterEvent args)
    {
        var endEv = new CP14EndCastMagicEffectEvent(args.User);
        RaiseLocalEvent(ent, ref endEv);

        if (args.Cancelled || args.Handled)
            return;

        EntityManager.TryGetEntity(args.TargetEntity, out var targetEnt);
        EntityCoordinates? targetPos = null;
        if (targetEnt is not null) { targetPos = Transform(targetEnt.Value).Coordinates; }

        CastSpell(ent, new CP14SpellEffectBaseArgs(args.User, targetEnt, targetPos), args.Cooldown ?? 0);

        args.Handled = true;
    }
}
