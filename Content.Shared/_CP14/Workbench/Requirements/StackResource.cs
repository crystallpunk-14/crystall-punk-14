/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CP14.Trading.Systems;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Workbench.Requirements;

public sealed partial class StackResource : CP14WorkbenchCraftRequirement
{
    [DataField(required: true)]
    public ProtoId<StackPrototype> Stack;

    [DataField]
    public int Count = 1;

    public override bool CheckRequirement(EntityManager entManager,
        IPrototypeManager protoManager,
        HashSet<EntityUid> placedEntities)
    {
        var count = 0;
        foreach (var ent in placedEntities)
        {
            if (!entManager.TryGetComponent<StackComponent>(ent, out var stack))
                continue;

            if (stack.StackTypeId != Stack)
                continue;

            count += stack.Count;
        }

        if (count < Count)
            return false;

        return true;
    }

    public override void PostCraft(EntityManager entManager, IPrototypeManager protoManager,
        HashSet<EntityUid> placedEntities)
    {
        var stackSystem = entManager.System<SharedStackSystem>();

        var requiredCount = Count;
        foreach (var placedEntity in placedEntities)
        {
            if (!entManager.TryGetComponent<StackComponent>(placedEntity, out var stack))
                continue;

            if (stack.StackTypeId != Stack)
                continue;

            var count = (int)MathF.Min(requiredCount, stack.Count);

            if (stack.Count - count <= 0)
                entManager.DeleteEntity(placedEntity);
            else
                stackSystem.SetCount(placedEntity, stack.Count - count, stack);

            requiredCount -= count;
        }
    }

    public override double GetPrice(EntityManager entManager,
        IPrototypeManager protoManager)
    {
        if (!protoManager.TryIndex(Stack, out var indexedStack))
            return 0;

        if (!protoManager.TryIndex(indexedStack.Spawn, out var indexedProto))
            return 0;

        var priceSys = entManager.System<CP14SharedStationEconomySystem>();

        return priceSys.GetEstimatedPrice(indexedProto) * Count;
    }

    public override string GetRequirementTitle(IPrototypeManager protoManager)
    {
        if (!protoManager.TryIndex(Stack, out var indexedStack))
            return "Error stack";

        return $"{Loc.GetString(indexedStack.Name)} x{Count}";
    }

    public override SpriteSpecifier? GetRequirementTexture(IPrototypeManager protoManager)
    {
        return !protoManager.TryIndex(Stack, out var indexedStack) ? null : indexedStack.Icon;
    }
}
