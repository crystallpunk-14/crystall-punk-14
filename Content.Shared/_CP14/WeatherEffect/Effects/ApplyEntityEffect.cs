using Content.Shared.EntityEffects;
using Robust.Shared.Random;

namespace Content.Shared._CP14.WeatherEffect.Effects;

public sealed partial class ApplyEntityEffect : Effects.CP14WeatherEffect
{
    [DataField(required: true, serverOnly: true)]
    public List<EntityEffect> Effects = new();

    public override void ApplyEffect(IEntityManager entManager, IRobustRandom random, EntityUid target)
    {
        if (!random.Prob(Prob))
            return;

        foreach (var effect in Effects)
        {
            effect.Effect(new EntityEffectBaseArgs(target, entManager));
        }
    }
}
