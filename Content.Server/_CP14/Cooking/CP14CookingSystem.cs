using System.Linq;
using Content.Server.Temperature.Systems;
using Content.Shared._CP14.Cooking;
using Content.Shared._CP14.Cooking.Components;
using Content.Shared._CP14.Temperature;

namespace Content.Server._CP14.Cooking;

public sealed class CP14CookingSystem : CP14SharedCookingSystem
{
    [Dependency] private readonly TemperatureSystem _temperature = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14FoodCookerComponent, CP14CookingDoAfter>(OnCookFinished);
    }

    private void OnCookFinished(Entity<CP14FoodCookerComponent> ent, ref CP14CookingDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!_proto.TryIndex(args.Recipe, out var indexedRecipe))
            return;

        TryTransformAll(ent);
        CookFood(ent, indexedRecipe);
        StopCooking(ent);

        args.Handled = true;
    }

    private void TryTransformAll(Entity<CP14FoodCookerComponent> ent)
    {
        if (!_container.TryGetContainer(ent, ent.Comp.ContainerId, out var container))
            return;

        var containedEntities = container.ContainedEntities.ToList();

        foreach (var contained in containedEntities)
        {
            if (!TryComp<CP14TemperatureTransformationComponent>(contained, out var transformable))
                continue;

            if (!transformable.AutoTransformOnCooked)
                continue;

            if (transformable.Entries.Count == 0)
                continue;

            var entry = transformable.Entries[0];

            _temperature.ForceChangeTemperature(contained, entry.TemperatureRange.X);
        }
    }
}
