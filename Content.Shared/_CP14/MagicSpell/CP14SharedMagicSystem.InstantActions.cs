using Content.Shared._CP14.MagicSpell.Components;
using Content.Shared._CP14.MagicSpell.Events;
using Content.Shared._CP14.MagicSpell.Spells;

namespace Content.Shared._CP14.MagicSpell;

public abstract partial class CP14SharedMagicSystem
{
    private void InitializeInstantActions()
    {
       SubscribeLocalEvent<CP14InstantActionEvent>(OnMagicInstantAction);
       SubscribeLocalEvent<CP14EntityWorldTargetActionEvent>(OnMagicEntityWorldTargetAction);
       SubscribeLocalEvent<CP14EntityTargetActionEvent>(OnMagicEntityTargetAction);
    }

    private void OnMagicInstantAction(CP14InstantActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<CP14MagicEffectComponent>(args.Action, out var magicEffect))
            return;

        if (!CanCastSpell((args.Action, magicEffect), args.Performer))
            return;

        var spellArgs =
            new CP14SpellEffectBaseArgs(args.Performer, magicEffect.SpellStorage, args.Performer, Transform(args.Performer).Coordinates);

        CastSpell((args.Action, magicEffect), spellArgs);
        _action.CP14StartCustomDelay(args.Action, args.Cooldown);
    }

    private void OnMagicEntityWorldTargetAction(CP14EntityWorldTargetActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<CP14MagicEffectComponent>(args.Action, out var magicEffect))
            return;

        if (!CanCastSpell((args.Action, magicEffect), args.Performer))
            return;

        var spellArgs =
            new CP14SpellEffectBaseArgs(args.Performer, magicEffect.SpellStorage, args.Entity, args.Coords);

        CastSpell((args.Action, magicEffect), spellArgs);
        _action.CP14StartCustomDelay(args.Action, args.Cooldown);
    }

    private void OnMagicEntityTargetAction(CP14EntityTargetActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<CP14MagicEffectComponent>(args.Action, out var magicEffect))
            return;

        if (!CanCastSpell((args.Action, magicEffect), args.Performer))
            return;

        var spellArgs =
            new CP14SpellEffectBaseArgs(args.Performer, magicEffect.SpellStorage, args.Target, Transform(args.Target).Coordinates);

        CastSpell((args.Action, magicEffect), spellArgs);
        _action.CP14StartCustomDelay(args.Action, args.Cooldown);
    }
}
