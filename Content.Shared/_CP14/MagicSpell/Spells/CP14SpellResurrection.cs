using Content.Shared.EntityEffects;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellResurrectionEffect : CP14SpellEffect
{
    [DataField(required: true, serverOnly: true)]
    public List<EntityEffect> Effects = new();

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.Target is null)
            return;

        var targetEntity = args.Target.Value;

        var mobState = entManager.System<MobStateSystem>();

        if (mobState.IsDead(targetEntity))
            mobState.ChangeMobState(targetEntity, MobState.Critical);
    }
}
