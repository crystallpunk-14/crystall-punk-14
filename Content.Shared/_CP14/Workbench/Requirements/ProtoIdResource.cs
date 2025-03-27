/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CP14.Workbench.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Workbench.Requirements;

public sealed partial class ProtoIdResource : CP14WorkbenchCraftRequirement
{
    public override bool HideRecipe { get; set; } = false;

    [DataField(required: true)]
    public EntProtoId ProtoId;

    [DataField]
    public int Count = 1;

    public override bool CheckRequirement(EntityManager entManager,
        IPrototypeManager protoManager,
        HashSet<EntityUid> placedEntities,
        EntityUid user,
        CP14WorkbenchRecipePrototype recipe)
    {
        var indexedIngredients = IndexIngredients(entManager, placedEntities);

        return indexedIngredients.TryGetValue(ProtoId, out var availableQuantity) && availableQuantity >= Count;
    }

    public override void PostCraft(EntityManager entManager,
        HashSet<EntityUid> placedEntities,
        EntityUid user)
    {
        var requiredCount = Count;

        foreach (var placedEntity in placedEntities)
        {
            if (!entManager.TryGetComponent<MetaDataComponent>(placedEntity, out var metaData))
                continue;

            if (metaData.EntityPrototype is null)
                continue;

            var placedProto = metaData.EntityPrototype.ID;
            if (placedProto != ProtoId || requiredCount <= 0)
                continue;

            requiredCount--;
            entManager.DeleteEntity(placedEntity);
        }
    }

    public override string GetRequirementTitle(IPrototypeManager protoManager)
    {
        if (!protoManager.TryIndex(ProtoId, out var indexedProto))
            return "Error entity";

        return $"{indexedProto.Name} x{Count}";
    }

    public override EntityPrototype? GetRequirementEntityView(IPrototypeManager protoManager)
    {
        if (!protoManager.TryIndex(ProtoId, out var indexedProto))
            return null;

        return indexedProto;
    }

    public override SpriteSpecifier? GetRequirementTexture(IPrototypeManager protoManager)
    {
        return null;
    }

    private Dictionary<EntProtoId, int> IndexIngredients(EntityManager entManager, HashSet<EntityUid> ingredients)
    {
        var indexedIngredients = new Dictionary<EntProtoId, int>();

        foreach (var ingredient in ingredients)
        {
            if (!entManager.TryGetComponent<MetaDataComponent>(ingredient, out var metaData))
                continue;

            var protoId = metaData.EntityPrototype?.ID;
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
