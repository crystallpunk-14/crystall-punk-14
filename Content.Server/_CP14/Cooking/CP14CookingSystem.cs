/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using System.Linq;
using Content.Server.Temperature.Systems;
using Content.Shared._CP14.Cooking;
using Content.Shared._CP14.Cooking.Components;
using Content.Shared._CP14.Temperature;
using Robust.Shared.Random;

namespace Content.Server._CP14.Cooking;

public sealed class CP14CookingSystem : CP14SharedCookingSystem
{
    [Dependency] private readonly TemperatureSystem _temperature = default!;

    protected override void OnCookBurned(Entity<CP14FoodCookerComponent> ent, ref CP14BurningDoAfter args)
    {
        base.OnCookBurned(ent, ref args);

        if (_random.Prob(ent.Comp.BurntAdditionalSpawnProb))
            Spawn(ent.Comp.BurntAdditionalSpawn, Transform(ent).Coordinates);
    }

    protected override void OnCookFinished(Entity<CP14FoodCookerComponent> ent, ref CP14CookingDoAfter args)
    {
        base.OnCookFinished(ent, ref args);

        if (args.Cancelled || args.Handled)
            return;

        TryTransformAll(ent);
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
