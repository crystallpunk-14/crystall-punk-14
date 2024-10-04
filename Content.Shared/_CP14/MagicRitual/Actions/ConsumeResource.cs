using System.Text;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitual.Actions;

public sealed partial class ConsumeResource : CP14RitualAction
{
    [DataField]
    public float CheckRange = 1f;

    [DataField]
    public Dictionary<EntProtoId, int> RequiredEntities = new ();

    [DataField]
    public Dictionary<ProtoId<StackPrototype>, int> RequiredStacks = new();

    public override string? GetGuidebookEffectDescription(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        var sb = new StringBuilder();
        sb.Append(Loc.GetString("cp14-ritual-effect-consume-resource", ("range", CheckRange)) + "\n");

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

    public override void Effect(EntityManager entManager, SharedTransformSystem transform, Entity<CP14MagicRitualPhaseComponent> phase)
    {
        var lookup = entManager.System<EntityLookupSystem>();
        var stack = entManager.System<SharedStackSystem>();

        var entitiesAround = lookup.GetEntitiesInRange(phase, CheckRange, LookupFlags.Uncontained);

        foreach (var reqEnt in RequiredEntities)
        {
            var requiredCount = reqEnt.Value;

            foreach (var entity in entitiesAround)
            {
                if (!entManager.TryGetComponent<MetaDataComponent>(entity, out var metaData))
                    continue;
                if (!entManager.HasComponent<TransformComponent>(entity))
                    continue;

                var entProto = metaData.EntityPrototype;
                if (entProto is null)
                    continue;

                if (entProto.ID == reqEnt.Key && requiredCount > 0)
                {
                    if (VisualEffect is not null)
                        entManager.Spawn(VisualEffect.Value, transform.GetMapCoordinates(entity));

                    entManager.DeleteEntity(entity);

                    requiredCount--;
                }
            }
        }

        foreach (var reqStack in RequiredStacks)
        {
            var requiredCount = reqStack.Value;

            foreach (var entity in entitiesAround)
            {
                if (!entManager.TryGetComponent<StackComponent>(entity, out var stackComp))
                    continue;

                if (stackComp.StackTypeId != reqStack.Key)
                    continue;

                var count = (int)MathF.Min(requiredCount, stackComp.Count);


                    if (stackComp.Count - count <= 0)
                        entManager.DeleteEntity(entity);
                    else
                        stack.SetCount(entity, stackComp.Count - count, stackComp);


                requiredCount -= count;

                if (VisualEffect is not null)
                    entManager.Spawn(VisualEffect.Value, transform.GetMapCoordinates(entity));
            }
        }
    }
}
