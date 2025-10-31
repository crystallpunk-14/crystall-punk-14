using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellToggleStatusEffect : CP14SpellEffect
{
    [DataField(required: true)]
    public EntProtoId StatusEffect = default;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.Target is null)
            return;

        var effectSys = entManager.System<StatusEffectsSystem>();

        if (!effectSys.HasStatusEffect(args.Target.Value, StatusEffect))
            effectSys.TrySetStatusEffectDuration(args.Target.Value, StatusEffect);
        else
            effectSys.TryRemoveStatusEffect(args.Target.Value, StatusEffect);

    }
}
