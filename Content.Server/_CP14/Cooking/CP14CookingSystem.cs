/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using System.Linq;
using System.Numerics;
using Content.Server.Nutrition.Components;
using Content.Server.Temperature.Systems;
using Content.Shared._CP14.Cooking;
using Content.Shared._CP14.Cooking.Components;
using Content.Shared._CP14.Temperature;
using Content.Shared.Chemistry.EntitySystems;
using Robust.Shared.Random;

namespace Content.Server._CP14.Cooking;

public sealed class CP14CookingSystem : CP14SharedCookingSystem
{
    [Dependency] private readonly TemperatureSystem _temperature = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14RandomFoodDataComponent, MapInitEvent>(OnRandomFoodMapInit);

        SubscribeLocalEvent<CP14FoodHolderComponent, SolutionContainerChangedEvent>(OnHolderChanged);
    }

    protected override bool TryTransferFood(Entity<CP14FoodHolderComponent> target, Entity<CP14FoodHolderComponent> source)
    {
        if (base.TryTransferFood(target, source))
        {
            //Sliceable
            if (source.Comp.FoodData?.SliceProto is not null)
            {
                var sliceable = EnsureComp<SliceableFoodComponent>(target);
                sliceable.Slice = source.Comp.FoodData.SliceProto;
                sliceable.TotalCount = source.Comp.FoodData.SliceCount;
            }
        }

        return true;
    }

    private void OnHolderChanged(Entity<CP14FoodHolderComponent> ent, ref SolutionContainerChangedEvent args)
    {
        if (args.Solution.Volume != 0)
            return;

        ent.Comp.FoodData = null;
        Dirty(ent);
    }

    private void OnRandomFoodMapInit(Entity<CP14RandomFoodDataComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<CP14FoodHolderComponent>(ent, out var holder))
            return;

        if (!_random.Prob(ent.Comp.Prob))
            return;

        var filteredRecipes = OrderedRecipes.Where(r => r.FoodType == holder.FoodType).ToList();
        if (filteredRecipes.Count == 0)
        {
            Log.Error($"No recipes found for food type {holder.FoodType}");
            return;
        }

        var randomFood = _random.Pick(filteredRecipes);

        UpdateFoodDataVisuals((ent, holder), randomFood.FoodData, ent.Comp.Rename);

        Dirty(ent.Owner, holder);
    }

    protected override void OnCookBurned(Entity<CP14FoodCookerComponent> ent, ref CP14BurningDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        base.OnCookBurned(ent, ref args);

        if (_random.Prob(ent.Comp.BurntAdditionalSpawnProb))
            Spawn(ent.Comp.BurntAdditionalSpawn, Transform(ent).Coordinates);
    }

    protected override void UpdateFoodDataVisuals(Entity<CP14FoodHolderComponent> ent, CP14FoodData data, bool rename = true)
    {
        base.UpdateFoodDataVisuals(ent, data, rename);

        if (ent.Comp.FoodData?.SliceProto is null)
            return;

        if (!TryComp<SliceableFoodComponent>(ent, out var sliceable))
            return;

        sliceable.Slice = ent.Comp.FoodData.SliceProto;
        sliceable.TotalCount = ent.Comp.FoodData.SliceCount;
    }

    protected override void OnCookFinished(Entity<CP14FoodCookerComponent> ent, ref CP14CookingDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        //We need transform all BEFORE Shared cooking code
        TryTransformAll(ent);

        base.OnCookFinished(ent, ref args);
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

            var newTemp = (entry.TemperatureRange.X + entry.TemperatureRange.Y) / 2;
            _temperature.ForceChangeTemperature(contained, newTemp);
        }
    }
}
