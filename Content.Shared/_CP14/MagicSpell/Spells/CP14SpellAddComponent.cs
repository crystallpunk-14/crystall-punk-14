using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellAddComponent : CP14SpellEffect
{
    [DataField]
    public ComponentRegistry Components = new();

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.Target is null)
            return;

        entManager.AddComponents(args.Target.Value, Components);
    }
}
