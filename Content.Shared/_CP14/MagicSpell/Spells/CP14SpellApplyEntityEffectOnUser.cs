using Content.Shared.EntityEffects;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellApplyEntityEffectOnUser : CP14SpellEffect
{
    [DataField(required: true, serverOnly: true)]
    public List<EntityEffect> Effects = new();

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.User == null)
            return;

        foreach (var effect in Effects)
        {
            effect.Effect(new EntityEffectBaseArgs(args.User.Value, entManager));
        }
    }
}
