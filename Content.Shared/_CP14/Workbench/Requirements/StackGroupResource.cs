/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CP14.Trading.Systems;
using Content.Shared._CP14.Workbench.Prototypes;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Workbench.Requirements;

public sealed partial class StackGroupResource : CP14WorkbenchCraftRequirement
{
    [DataField(required: true)]
    public ProtoId<CP14StackGroupPrototype> Group;

    [DataField]
    public int Count = 1;

    public override bool CheckRequirement(IEntityManager entManager,
        IPrototypeManager protoManager,
        HashSet<EntityUid> placedEntities)
    {
        if (!protoManager.TryIndex(Group, out var indexedGroup))
            return false;

        var count = 0;
        foreach (var ent in placedEntities)
        {
            if (!entManager.TryGetComponent<StackComponent>(ent, out var stack))
                continue;

            if (!indexedGroup.Stacks.Contains(stack.StackTypeId))
                continue;

            count += stack.Count;
        }

        if (count < Count)
            return false;

        return true;
    }

    public override void PostCraft(IEntityManager entManager, IPrototypeManager protoManager,
        HashSet<EntityUid> placedEntities)
    {
        var stackSystem = entManager.System<SharedStackSystem>();

        if (!protoManager.TryIndex(Group, out var indexedGroup))
            return;

        var requiredCount = Count;
        foreach (var placedEntity in placedEntities)
        {
            if (!entManager.TryGetComponent<StackComponent>(placedEntity, out var stack))
                continue;

            if (!indexedGroup.Stacks.Contains(stack.StackTypeId))
                continue;

            var count = (int)MathF.Min(requiredCount, stack.Count);

            if (stack.Count - count <= 0)
                entManager.DeleteEntity(placedEntity);
            else
                stackSystem.SetCount(placedEntity, stack.Count - count, stack);

            requiredCount -= count;
        }
    }

    /// <summary>
    /// We take the cheapest material from the group for evaluation.
    /// </summary>
    public override double GetPrice(IEntityManager entManager, IPrototypeManager protoManager)
    {
        if (!protoManager.TryIndex(Group, out var indexedGroup))
            return 0;

        double price = 0;
        foreach (var stack in indexedGroup.Stacks)
        {
            if (!protoManager.TryIndex(stack, out var indexedStack))
                continue;

            if (!protoManager.TryIndex(indexedStack.Spawn, out var indexedProto))
                continue;


            var priceSys = entManager.System<CP14SharedStationEconomySystem>();

            var tempPrice = priceSys.GetEstimatedPrice(indexedProto);
            if (price > 0)
            {
                price = Math.Min(price, tempPrice);
            }
            else
            {
                price = tempPrice;
            }
        }

        return price * Count;
    }

    public override string GetRequirementTitle(IPrototypeManager protoManager)
    {
        var indexedGroup = protoManager.Index(Group);

        return $"{Loc.GetString(indexedGroup.Name)} x{Count}";
    }

    public override SpriteSpecifier? GetRequirementTexture(IPrototypeManager protoManager)
    {
        var indexedGroup = protoManager.Index(Group);

        return !protoManager.TryIndex(indexedGroup.Stacks.FirstOrNull(), out var indexedStack) ? null : indexedStack.Icon;
    }
}
