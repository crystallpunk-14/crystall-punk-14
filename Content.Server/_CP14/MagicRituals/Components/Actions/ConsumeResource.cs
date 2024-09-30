using Content.Server.Stack;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.MagicRituals.Components.Actions;

public sealed partial class ConsumeResource : CP14RitualAction
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
    [DataField("effect")]
    public EntProtoId? VisualEffect = "CP14DustEffect";

    public override void Effect(EntityManager entManager, EntityUid phaseEnt)
    {
        var _lookup = entManager.System<EntityLookupSystem>();
        var _transform = entManager.System<SharedTransformSystem>();
        var _stack = entManager.System<StackSystem>();

        var entitiesAround = _lookup.GetEntitiesInRange(phaseEnt, CheckRange, LookupFlags.Uncontained);

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
                    if (VisualEffect is not null)
                        entManager.Spawn(VisualEffect.Value, _transform.GetMapCoordinates(entity));

                    entManager.DeleteEntity(entity);

                    requiredCount--;
                }
            }
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


                    if (stack.Count - count <= 0)
                        entManager.DeleteEntity(entity);
                    else
                        _stack.SetCount(entity, stack.Count - count, stack);


                requiredCount -= count;

                if (VisualEffect is not null)
                    entManager.Spawn(VisualEffect.Value, _transform.GetMapCoordinates(entity));
            }
        }
    }
}
