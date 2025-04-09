using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellSpawnEntityOnTarget : CP14SpellEffect
{
    [DataField]
    public List<EntProtoId> Spawns = new();

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        EntityCoordinates? targetPoint = null;
        if (args.Position is not null)
            targetPoint = args.Position.Value;
        if (args.Target is not null && entManager.TryGetComponent<TransformComponent>(args.Target.Value, out var transformComponent))
            targetPoint = transformComponent.Coordinates;

        if (targetPoint is null)
            return;

        foreach (var spawn in Spawns)
        {
            entManager.SpawnAtPosition(spawn, targetPoint.Value);
        }
    }
}
