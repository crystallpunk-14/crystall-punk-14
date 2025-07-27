/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CP14.Trading.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Workbench.Requirements;

public sealed partial class ProtoIdResource : CP14WorkbenchCraftRequirement
{
    [DataField(required: true)]
    public EntProtoId ProtoId;

    [DataField]
    public int Count = 1;

    public override bool CheckRequirement(IEntityManager entManager,
        IPrototypeManager protoManager,
        HashSet<EntityUid> placedEntities)
    {
        var indexedIngredients = IndexIngredients(entManager, placedEntities);

        return indexedIngredients.TryGetValue(ProtoId, out var availableQuantity) && availableQuantity >= Count;
    }

    public override void PostCraft(IEntityManager entManager,IPrototypeManager protoManager, HashSet<EntityUid> placedEntities)
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

    public override double GetPrice(IEntityManager entManager,
        IPrototypeManager protoManager)
    {
        if (!protoManager.TryIndex(ProtoId, out var indexedProto))
            return 0;

        var priceSys = entManager.System<CP14SharedStationEconomySystem>();

        return priceSys.GetEstimatedPrice(indexedProto) * Count;
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

    private Dictionary<EntProtoId, int> IndexIngredients(IEntityManager entManager, HashSet<EntityUid> ingredients)
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
