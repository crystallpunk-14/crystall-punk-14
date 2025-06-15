using Content.Shared._CP14.MagicSpell.Components;
using Content.Shared._CP14.MagicSpell.Events;
using Content.Shared._CP14.MagicSpell.Spells;

namespace Content.Shared._CP14.MagicSpell;

public abstract partial class CP14SharedMagicSystem
{
    private void InitializeInstantActions()
    {
       SubscribeLocalEvent<CP14InstantActionEvent>(OnMagicInstantAction);
       SubscribeLocalEvent<CP14WorldTargetActionEvent>(OnMagicWorldTargetAction);
       SubscribeLocalEvent<CP14EntityTargetActionEvent>(OnMagicEntityTargetAction);
    }

    private void OnMagicInstantAction(CP14InstantActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<CP14MagicEffectComponent>(args.Action, out var magicEffect))
            return;

        var spellArgs = new CP14SpellEffectBaseArgs(args.Performer, magicEffect.SpellStorage, args.Performer, Transform(args.Performer).Coordinates);

        if (!CanCastSpell((args.Action, magicEffect), spellArgs))
            return;

        CastSpell((args.Action, magicEffect), spellArgs);
        _action.SetCooldown(args.Action.Owner, args.Cooldown);
    }

    private void OnMagicWorldTargetAction(CP14WorldTargetActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<CP14MagicEffectComponent>(args.Action, out var magicEffect))
            return;

        var spellArgs = new CP14SpellEffectBaseArgs(args.Performer, magicEffect.SpellStorage, null, args.Target);

        if (!CanCastSpell((args.Action, magicEffect), spellArgs))
            return;

        CastSpell((args.Action, magicEffect), spellArgs);
        _action.SetCooldown(args.Action.Owner, args.Cooldown);
    }

    private void OnMagicEntityTargetAction(CP14EntityTargetActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<CP14MagicEffectComponent>(args.Action, out var magicEffect))
            return;

        var spellArgs = new CP14SpellEffectBaseArgs(args.Performer, magicEffect.SpellStorage, args.Target, Transform(args.Target).Coordinates);

        if (!CanCastSpell((args.Action, magicEffect), spellArgs))
            return;

        CastSpell((args.Action, magicEffect), spellArgs);
        _action.SetCooldown(args.Action.Owner, args.Cooldown);
    }
}
