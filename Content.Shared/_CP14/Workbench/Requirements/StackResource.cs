/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CP14.Workbench.Prototypes;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Workbench.Requirements;

public sealed partial class StackResource : CP14WorkbenchCraftRequirement
{
    public override bool HideRecipe { get; set; } = false;

    [DataField(required: true)]
    public ProtoId<StackPrototype> Stack;

    [DataField]
    public int Count = 1;

    public override bool CheckRequirement(EntityManager entManager,
        IPrototypeManager protoManager,
        HashSet<EntityUid> placedEntities,
        EntityUid user,
        CP14WorkbenchRecipePrototype recipe)
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

    public override void PostCraft(EntityManager entManager,
        HashSet<EntityUid> placedEntities,
        EntityUid user)
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

    public override string GetRequirementTitle(IPrototypeManager protoManager)
    {
        if (!protoManager.TryIndex(Stack, out var indexedStack))
            return "Error stack";

        return $"{Loc.GetString(indexedStack.Name)} x{Count}";
    }

    public override EntityPrototype? GetRequirementEntityView(IPrototypeManager protoManager)
    {
        return null;
    }

    public override SpriteSpecifier? GetRequirementTexture(IPrototypeManager protoManager)
    {
        return !protoManager.TryIndex(Stack, out var indexedStack) ? null : indexedStack.Icon;
    }
}
