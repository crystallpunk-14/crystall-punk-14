using Content.Shared.Construction;
using Content.Shared.Construction.Conditions;
using JetBrains.Annotations;
using Robust.Shared.Map;

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
        var lookupSystem = entityManager.System<EntityLookupSystem>();

        foreach (var entity in location.GetEntitiesInTile(LookupFlags.Static))
        {
            if (entityManager.HasComponent<CP14RoofComponent>(entity))
                return true;
        }

        return false;
    }
}