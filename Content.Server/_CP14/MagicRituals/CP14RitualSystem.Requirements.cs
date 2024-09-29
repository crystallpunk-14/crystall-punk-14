using Content.Server._CP14.MagicRituals.Components.Requirements;
using Content.Shared._CP14.MagicRitual;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.MagicRituals;

public sealed partial class CP14RitualSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    private EntityQuery<MetaDataComponent> _metaQuery;
    private EntityQuery<StackComponent> _stackQuery;

    private void InitializeRequirements()
    {
        SubscribeLocalEvent<CP14RitualRequirementEntitiesComponent, CP14RitualTriggerAttempt>(OnResourceRequirementCheck);
    }

    private void OnResourceRequirementCheck(Entity<CP14RitualRequirementEntitiesComponent> requirement, ref CP14RitualTriggerAttempt args)
    {
        var entitiesAround = _lookup.GetEntitiesInRange(requirement, requirement.Comp.CheckRange, LookupFlags.Uncontained);

        var passed = true;

        foreach (var reqEnt in requirement.Comp.RequiredEntities)
        {
            var requiredCount = reqEnt.Value;

            foreach (var entity in entitiesAround)
            {
                var entProto = MetaData(entity).EntityPrototype;
                if (entProto is null)
                    continue;

                if (entProto.ID == reqEnt.Key && requiredCount > 0)
                {
                    Spawn(requirement.Comp.VisualEffect, Transform(entity).Coordinates);

                    requiredCount--;
                    if (requirement.Comp.ConsumeResource)
                        Del(entity);
                }
            }

            if (requiredCount > 0)
                passed = false;
        }

        foreach (var reqStack in requirement.Comp.RequiredStack)
        {
            var requiredCount = reqStack.Value;

            foreach (var entity in entitiesAround)
            {
                if (!_stackQuery.TryGetComponent(entity, out var stack))
                    continue;

                if (stack.StackTypeId != reqStack.Key)
                    continue;

                var count = (int)MathF.Min(requiredCount, stack.Count);
                requiredCount -= count;

                Spawn(requirement.Comp.VisualEffect, Transform(entity).Coordinates);

                if (requirement.Comp.ConsumeResource)
                {
                    if (stack.Count - count <= 0)
                        Del(entity);
                    else
                        _stack.SetCount(entity, stack.Count - count, stack);
                }
            }

            if (requiredCount > 0)
                passed = false;
        }

        if (!passed)
            args.Cancel();
    }
    private Dictionary<EntProtoId, int> IndexIngredients(HashSet<EntityUid> ingredients)
    {
        var indexedIngredients = new Dictionary<EntProtoId, int>();

        foreach (var ingredient in ingredients)
        {
            var protoId = _metaQuery.GetComponent(ingredient).EntityPrototype?.ID;
            if (protoId == null)
                continue;

            if (indexedIngredients.ContainsKey(protoId))
                indexedIngredients[protoId]++;
            else
                indexedIngredients[protoId] = 1;
        }
        return indexedIngredients;
    }
}
