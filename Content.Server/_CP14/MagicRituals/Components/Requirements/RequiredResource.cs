using Content.Shared.Stacks;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.MagicRituals.Components.Requirements;

/// <summary>
/// Requires certain specific entities to be near the ritual. TODO: Replace with Whitelist
/// </summary>
public sealed partial class RequiredResource : CP14RitualRequirement
{
    [DataField]
    public float CheckRange = 1f;

    [DataField]
    public Dictionary<EntProtoId, int> RequiredEntities = new ();

    [DataField]
    public Dictionary<ProtoId<StackPrototype>, int> RequiredStack = new();

    /// <summary>
    /// Effect appearing in place of used entities
    /// </summary>
    [DataField]
    public EntProtoId? Effect = "CP14DustEffect";

    public override bool Check(EntityManager entManager, Entity<CP14MagicRitualPhaseComponent> phaseEnt, float stability)
    {
        var _lookup = entManager.System<EntityLookupSystem>();
        var _transform = entManager.System<SharedTransformSystem>();

        var entitiesAround = _lookup.GetEntitiesInRange(phaseEnt, CheckRange, LookupFlags.Uncontained);

        var passed = true;

        foreach (var reqEnt in RequiredEntities)
        {
            var requiredCount = reqEnt.Value;

            foreach (var entity in entitiesAround)
            {
                if (!entManager.TryGetComponent<MetaDataComponent>(entity, out var metaData))
                    continue;
                if (!entManager.TryGetComponent<TransformComponent>(entity, out var xform))
                    continue;

                var entProto = metaData.EntityPrototype;
                if (entProto is null)
                    continue;

                if (entProto.ID == reqEnt.Key && requiredCount > 0)
                {
                    if (Effect is not null)
                        entManager.Spawn(Effect.Value, _transform.GetMapCoordinates(entity));

                    requiredCount--;
                }
            }

            if (requiredCount > 0)
                passed = false;
        }

        foreach (var reqStack in RequiredStack)
        {
            var requiredCount = reqStack.Value;

            foreach (var entity in entitiesAround)
            {
                if (!entManager.TryGetComponent<StackComponent>(entity, out var stack))
                    continue;

                if (stack.StackTypeId != reqStack.Key)
                    continue;

                var count = (int)MathF.Min(requiredCount, stack.Count);
                requiredCount -= count;

                if (Effect is not null)
                    entManager.Spawn(Effect.Value, _transform.GetMapCoordinates(entity));
            }

            if (requiredCount > 0)
                passed = false;
        }

        return passed;
    }
}
