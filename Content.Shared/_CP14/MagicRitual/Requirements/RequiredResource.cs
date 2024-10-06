using System.Text;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitual.Requirements;

/// <summary>
/// Requires certain specific entities to be near the ritual. TODO: Replace with Whitelist
/// </summary>
public sealed partial class RequiredResource : CP14RitualRequirement
{
    [DataField]
    public float CheckRange = 3f;

    [DataField]
    public Dictionary<EntProtoId, int> RequiredEntities = new ();

    [DataField]
    public Dictionary<ProtoId<StackPrototype>, int> RequiredStacks = new();

    /// <summary>
    /// Effect appearing in place of used entities
    /// </summary>
    [DataField("vfx")]
    public EntProtoId? Effect = "CP14DustEffect";

    public override string? GetGuidebookRequirementDescription(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        var sb = new StringBuilder();
        sb.Append(Loc.GetString("cp14-ritual-required-resource", ("range", CheckRange)) + "\n");

        foreach (var entity in RequiredEntities)
        {
            if (!prototype.TryIndex(entity.Key, out var indexed))
                continue;

            sb.Append(Loc.GetString("cp14-ritual-entry-item", ("name", indexed.Name), ("count", entity.Value)) + "\n");
        }

        foreach (var stack in RequiredStacks)
        {
            if (!prototype.TryIndex(stack.Key, out var indexed))
                continue;

            sb.Append(Loc.GetString("cp14-ritual-entry-item", ("name", Loc.GetString(indexed.Name)), ("count", stack.Value)) + "\n");
        }

        return sb.ToString();
    }

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

        foreach (var reqStack in RequiredStacks)
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
