using Content.Shared.Stealth;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellRevealStealthUser : CP14SpellEffect
{
    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.User is null)
            return;

        var stealth = entManager.System<SharedStealthSystem>();

        stealth.SetVisibility(args.User.Value, 1);
    }
}
