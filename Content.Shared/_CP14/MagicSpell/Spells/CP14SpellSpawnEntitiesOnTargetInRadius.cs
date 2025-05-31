using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellSpawnEntitiesOnTargetInRadius : CP14SpellEffect
{
    [DataField]
    public EntProtoId Spawn = new();

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        EntityCoordinates? targetPoint = null;
        if (args.Position is not null)
            targetPoint = args.Position.Value;
        if (args.Target is not null && entManager.TryGetComponent<TransformComponent>(args.Target.Value, out var transformComponent))
            targetPoint = transformComponent.Coordinates;

        if (targetPoint is null)
            return;

        // Spawn in center
        entManager.SpawnAtPosition(Spawn, targetPoint.Value);

        //Spawn in other directions
        for (var i = 0; i < 4; i++)
        {
            var direction = (DirectionFlag) (1 << i);
            var coords = targetPoint.Value.Offset(direction.AsDir().ToVec());

            entManager.SpawnAtPosition(Spawn, coords);
        }
    }
}
