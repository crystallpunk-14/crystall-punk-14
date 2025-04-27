using Content.Shared._CP14.StatusEffect;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellApplyStatusEffect : CP14SpellEffect
{
    [DataField(required: true)]
    public EntProtoId StatusEffect;

    [DataField]
    public TimeSpan? Duration;

    [DataField]
    public bool ResetCooldown = true;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.Target is null)
            return;

        var targetEntity = args.Target.Value;

        var statusEffectSys = entManager.System<CP14SharedStatusEffectSystem>();

        statusEffectSys.TryAddStatusEffect(targetEntity, StatusEffect, Duration, ResetCooldown);
    }
}
