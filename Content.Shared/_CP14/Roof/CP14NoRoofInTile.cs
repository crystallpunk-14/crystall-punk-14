using Content.Shared.Construction;
using Content.Shared.Construction.Conditions;
using JetBrains.Annotations;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Shared._CP14.Roof;

[UsedImplicitly]
[DataDefinition]
public sealed partial class CP14NoRoofInTile : IConstructionCondition
{
    public ConstructionGuideEntry GenerateGuideEntry()
    {
        return new ConstructionGuideEntry
        {
            Localization = "cp14-construction-step-condition-no-roof-in-tile",
        };
    }

    public bool Condition(EntityUid user, EntityCoordinates location, Direction direction)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var mapSystem = entityManager.System<SharedMapSystem>();
        var transformSystem = entityManager.System<SharedTransformSystem>();

        var grid = transformSystem.GetGrid(user);

        if (grid == null || !entityManager.TryGetComponent<MapGridComponent>(grid, out var gridComp))
        {
            return false;
        }

        var targetPos = transformSystem.ToMapCoordinates(location);
        var anchored = mapSystem.GetAnchoredEntities(grid.Value, gridComp, targetPos);

        foreach (var entt in anchored)
        {
            if (entityManager.HasComponent<CP14RoofComponent>(entt))
            {
                return false;
            }
        }

        return true;
    }
}