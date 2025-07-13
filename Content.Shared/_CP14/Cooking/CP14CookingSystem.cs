using System.Linq;
using Content.Shared._CP14.Cooking.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Temperature;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Cooking;

public sealed class CP14CookingSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;

    /// <summary>
    /// Stores a list of all recipes sorted by complexity: the most complex ones at the beginning.
    /// When attempting to cook, the most complex recipes will be checked first,
    /// gradually moving down to the easiest ones.
    /// The easiest recipes are usually the most “abstract,”
    /// so they will be suitable for the largest number of recipes.
    /// </summary>
    private List<(EntityPrototype, CP14FoodRecipeComponent)> _orderedRecipes = [];

    public override void Initialize()
    {
        base.Initialize();

        CacheAndOrderRecipes();

        SubscribeLocalEvent<CP14FoodCookerComponent, OnTemperatureChangeEvent>(OnTemperatureChange);
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs ev)
    {
        if (!ev.WasModified<EntityPrototype>())
            return;

        CacheAndOrderRecipes();
    }

    private void CacheAndOrderRecipes()
    {

        foreach (var ent in _proto.EnumeratePrototypes<EntityPrototype>())
        {
            if (!ent.TryGetComponent(out CP14FoodRecipeComponent? recipe, Factory))
                continue;

            _orderedRecipes.Add((ent, recipe));
        }

        _orderedRecipes.Sort((a, b) =>
        {
            var aComplexity = a.Item2.Conditions.Sum(condition => condition.GetComplexity());
            var bComplexity = b.Item2.Conditions.Sum(condition => condition.GetComplexity());
            return bComplexity.CompareTo(aComplexity); // Sort descending
        });
    }

    private void OnTemperatureChange(Entity<CP14FoodCookerComponent> ent, ref OnTemperatureChangeEvent args)
    {
        if (args.CurrentTemperature > 500)
        {
            var recipe = GetRecipe(ent);
            if (recipe is not null)
                CookFood(ent, recipe.Value);
        }
    }

    private (EntityPrototype, CP14FoodRecipeComponent)? GetRecipe(Entity<CP14FoodCookerComponent> ent)
    {
        _container.TryGetContainer(ent, ent.Comp.ContainerId, out var container);
        _solution.TryGetSolution(ent.Owner, ent.Comp.SolutionId, out _, out var solution);

        if (_orderedRecipes.Count == 0)
            throw new InvalidOperationException("No cooking recipes found. Please ensure that the CP14CookingRecipePrototype is defined and loaded.");
        (EntityPrototype, CP14FoodRecipeComponent)? selectedRecipe = null;
        foreach (var recipe in _orderedRecipes)
        {
            if (recipe.Item2.FoodType != ent.Comp.FoodType)
                continue;

            var conditionsMet = true;
            foreach (var condition in recipe.Item2.Conditions)
            {
                if (!condition.CheckRequirement(EntityManager, _proto, container?.ContainedEntities ?? [], solution))
                {
                    conditionsMet = false;
                    break;
                }
            }

            if (!conditionsMet)
                continue;

            selectedRecipe = recipe;
            break;
        }

        return selectedRecipe;
    }

    private void CookFood(Entity<CP14FoodCookerComponent> ent, (EntityPrototype, CP14FoodRecipeComponent) recipe)
    {
        _container.TryGetContainer(ent, ent.Comp.ContainerId, out var container);
        _solution.TryGetSolution(ent.Owner, ent.Comp.SolutionId, out var soln, out var solution);

        var newData = new CP14FoodData
        {
            Visuals = new List<PrototypeLayerData>(recipe.Item2.FoodData.Visuals),
            Trash = new List<EntProtoId>(recipe.Item2.FoodData.Trash),
            Flavors = new HashSet<LocId>(recipe.Item2.FoodData.Flavors),
            Name = recipe.Item2.FoodData.Name,
            Desc = recipe.Item2.FoodData.Desc,
            Solution = recipe.Item2.FoodData.Solution
        };

        newData.Name = recipe.Item1.Name;
        newData.Desc = recipe.Item1.Description;

        //Process entities
        foreach (var contained in container?.ContainedEntities ?? [])
        {
            if (TryComp<FoodComponent>(contained, out var food))
            {
                //Merge trash
                newData.Trash.AddRange(food.Trash);
            }
            else
            {
                //This item is not food, so its become trash
                var meta = MetaData(contained).EntityPrototype;
                if (meta is not null)
                    newData.Trash.Add(meta.ID);
            }

            if (TryComp<FlavorProfileComponent>(contained, out var flavorComp))
            {
                //Merge flavors
                foreach (var flavor in flavorComp.Flavors)
                {
                    newData.Flavors.Add(flavor);
                }
            }
            QueueDel(contained);
        }

        //Process solution
        if (soln is not null && solution is not null)
        {
            //Merge solution
            newData.Solution.MaxVolume += solution.Volume;
            newData.Solution.AddSolution(solution, _proto);

            _solution.RemoveAllSolution(soln.Value);
        }

        ent.Comp.FoodData = newData;
    }
}
