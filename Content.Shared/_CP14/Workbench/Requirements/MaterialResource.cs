/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CP14.Material;
using Content.Shared._CP14.Workbench.Prototypes;
using Content.Shared.Materials;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Workbench.Requirements;

public sealed partial class MaterialResource : CP14WorkbenchCraftRequirement
{
    public override bool HideRecipe { get; set; } = false;

    [DataField(required: true)]
    public ProtoId<MaterialPrototype> Material;

    [DataField]
    public int Count = 1;

    public override bool CheckRequirement(
        EntityManager entManager,
        IPrototypeManager protoManager,
        HashSet<EntityUid> placedEntities,
        EntityUid user)
    {
        var count = 0;
        foreach (var ent in placedEntities)
        {
            if (!entManager.TryGetComponent<CP14MaterialComponent>(ent, out var material))
                continue;

            entManager.TryGetComponent<StackComponent>(ent, out var stack);

            foreach (var (key, value) in material.Materials)
            {
                if (key != Material)
                    continue;

                if (stack is null)
                {
                    count += value;
                }
                else
                {
                    count += value * stack.Count;
                }
            }
        }

        if (count < Count)
            return false;

        return true;
    }

    public override void PostCraft(EntityManager entManager, IPrototypeManager protoManager, HashSet<EntityUid> placedEntities, EntityUid user)
    {
        var stackSystem = entManager.System<SharedStackSystem>();

        var requiredCount = Count;
        foreach (var placedEntity in placedEntities)
        {
            if (!entManager.TryGetComponent<CP14MaterialComponent>(placedEntity, out var material))
                continue;

            entManager.TryGetComponent<StackComponent>(placedEntity, out var stack);

            foreach (var mat in material.Materials)
            {
                if (mat.Key != Material)
                    continue;

                if (requiredCount <= 0)
                    return;

                if (stack is null)
                {
                    var value = (int)MathF.Min(requiredCount, mat.Value);
                    requiredCount -= value;
                    entManager.DeleteEntity(placedEntity);
                }
                else
                {
                    var materialValue = mat.Value * stack.Count;
                    var countToRemove = (int)MathF.Min(requiredCount, materialValue);
                    var newStackCount = (int)MathF.Ceiling((materialValue - countToRemove) / (float)mat.Value);

                    if (newStackCount <= 0)
                        entManager.DeleteEntity(placedEntity);
                    else
                        stackSystem.SetCount(placedEntity, newStackCount, stack);

                    requiredCount -= countToRemove;
                }
            }
        }
    }

    public override string GetRequirementTitle(IPrototypeManager protoManager)
    {
        if (!protoManager.TryIndex(Material, out var indexedMaterial))
            return "Error material";

        return $"{Loc.GetString(indexedMaterial.Name)} x{Count}";
    }

    public override EntityPrototype? GetRequirementEntityView(IPrototypeManager protoManager)
    {
        return null;
    }

    public override SpriteSpecifier? GetRequirementTexture(IPrototypeManager protoManager)
    {
        return !protoManager.TryIndex(Material, out var indexedMaterial) ? null : indexedMaterial.Icon;
    }
}
