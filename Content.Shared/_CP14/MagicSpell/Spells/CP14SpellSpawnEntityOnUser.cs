using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellSpawnEntityOnUser : CP14SpellEffect
{
    [DataField]
    public List<EntProtoId> Spawns = new();

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.User is null || !entManager.TryGetComponent<TransformComponent>(args.User.Value, out var transformComponent))
            return;

        foreach (var spawn in Spawns)
        {
            entManager.PredictedSpawnAtPosition(spawn, transformComponent.Coordinates);
        }
    }
}
