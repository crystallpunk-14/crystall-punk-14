using System.Linq;
using Content.Shared._CP14.Cooking.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Nutrition.Components;
using Content.Shared.Popups;
using Content.Shared.Storage.Components;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Temperature;
using Content.Shared.Throwing;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Cooking;

public abstract class CP14SharedCookingSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;

    /// <summary>
    /// Stores a list of all recipes sorted by complexity: the most complex ones at the beginning.
    /// When attempting to cook, the most complex recipes will be checked first,
    /// gradually moving down to the easiest ones.
    /// The easiest recipes are usually the most “abstract,”
    /// so they will be suitable for the largest number of recipes.
    /// </summary>
    private List<CP14CookingRecipePrototype> _orderedRecipes = [];

    public override void Initialize()
    {
        base.Initialize();

        CacheAndOrderRecipes();

        SubscribeLocalEvent<CP14FoodCookerComponent, OnTemperatureChangeEvent>(OnTemperatureChange);
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);
        SubscribeLocalEvent<CP14FoodCookerComponent, AfterInteractEvent>(OnInteractUsing);
        SubscribeLocalEvent<CP14FoodCookerComponent, ExaminedEvent>(OnExaminedEvent);
        SubscribeLocalEvent<CP14FoodHolderComponent, AfterInteractEvent>(OnAfterInteract);

        SubscribeLocalEvent<CP14FoodCookerComponent, ContainerIsInsertingAttemptEvent>(OnInsertAttempt);
        SubscribeLocalEvent<CP14FoodCookerComponent, LandEvent>(OnLand);
    }

    private void OnLand(Entity<CP14FoodCookerComponent> ent, ref LandEvent args)
    {
        ent.Comp.FoodData = null;
    }

    private void OnInsertAttempt(Entity<CP14FoodCookerComponent> ent, ref ContainerIsInsertingAttemptEvent args)
    {
        if (ent.Comp.FoodData is not null)
        {
            _popup.PopupEntity(Loc.GetString("cp14-cooking-popup-not-empty", ("name", MetaData(ent).EntityName)), ent);
            args.Cancel();
        }
    }

    private void OnExaminedEvent(Entity<CP14FoodCookerComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.FoodData is null || ent.Comp.FoodData.Name is null)
            return;

        var remaining = ent.Comp.FoodData.Solution.Volume;

        args.PushMarkup(Loc.GetString("cp14-cooking-examine",
            ("name", Loc.GetString(ent.Comp.FoodData.Name)),
            ("count", remaining)));
    }

    private void OnAfterInteract(Entity<CP14FoodHolderComponent> ent, ref AfterInteractEvent args)
    {
        if (!TryComp<CP14FoodCookerComponent>(args.Target, out var cooker))
            return;

        if (cooker.FoodData is null)
            return;

        if (ent.Comp.Visuals is not null)
            return;

        MoveFoodToHolder(ent, (args.Target.Value, cooker));
    }

    private void OnInteractUsing(Entity<CP14FoodCookerComponent> ent, ref AfterInteractEvent args)
    {
        if (!TryComp<CP14FoodHolderComponent>(args.Target, out var holder))
            return;

        if (holder.Visuals is not null)
            return;

        if (ent.Comp.FoodData is null)
            return;

        MoveFoodToHolder((args.Target.Value, holder), ent);
    }

    /// <summary>
    /// Transfer food data from cooker to holder
    /// </summary>
    private void MoveFoodToHolder(Entity<CP14FoodHolderComponent> ent, Entity<CP14FoodCookerComponent> cooker)
    {
        if (!TryComp<FoodComponent>(ent, out var foodComp))
            return;

        if (cooker.Comp.FoodData is null)
            return;

        //Solutions
        if (_solution.TryGetSolution(ent.Owner, foodComp.Solution, out var soln, out var solution))
        {
            if (solution.Volume > 0)
            {
                _popup.PopupEntity(Loc.GetString("cp14-cooking-popup-not-empty", ("name", MetaData(ent).EntityName)), ent);
                return;
            }

            _solution.TryTransferSolution(soln.Value, cooker.Comp.FoodData.Solution, solution.MaxVolume);
        }

        //Trash
        foodComp.Trash.AddRange(cooker.Comp.FoodData.Trash);

        //Name and Description
        if (cooker.Comp.FoodData.Name is not null)
            _metaData.SetEntityName(ent, Loc.GetString(cooker.Comp.FoodData.Name));
        if (cooker.Comp.FoodData.Desc is not null)
            _metaData.SetEntityDescription(ent, Loc.GetString(cooker.Comp.FoodData.Desc));

        //Flavors
        EnsureComp<FlavorProfileComponent>(ent, out var flavorComp);
        foreach (var flavor in cooker.Comp.FoodData.Flavors)
        {
            flavorComp.Flavors.Add(flavor);
        }

        //Visuals
        ent.Comp.Visuals = cooker.Comp.FoodData.Visuals;

        Dirty(ent);

        //Clear cooker data
        if (cooker.Comp.FoodData.Solution.Volume <= 0)
        {
            cooker.Comp.FoodData = null;
        }
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs ev)
    {
        if (!ev.WasModified<EntityPrototype>())
            return;

        CacheAndOrderRecipes();
    }

    private void CacheAndOrderRecipes()
    {
        _orderedRecipes = _proto.EnumeratePrototypes<CP14CookingRecipePrototype>()
            .OrderByDescending(recipe => recipe.Requirements.Sum(condition => condition.GetComplexity()))
            .ToList();
    }

    private void OnTemperatureChange(Entity<CP14FoodCookerComponent> ent, ref OnTemperatureChangeEvent args)
    {
        if (ent.Comp.FoodData is not null)
            return;

        if (args.CurrentTemperature > 500)
        {
            var recipe = GetRecipe(ent);
            if (recipe is not null)
                CookFood(ent, recipe);
        }
    }

    private CP14CookingRecipePrototype? GetRecipe(Entity<CP14FoodCookerComponent> ent)
    {
        _container.TryGetContainer(ent, ent.Comp.ContainerId, out var container);
        _solution.TryGetSolution(ent.Owner, ent.Comp.SolutionId, out _, out var solution);

        if (_orderedRecipes.Count == 0)
            throw new InvalidOperationException(
                "No cooking recipes found. Please ensure that the CP14CookingRecipePrototype is defined and loaded.");
        CP14CookingRecipePrototype? selectedRecipe = null;
        foreach (var recipe in _orderedRecipes)
        {
            if (recipe.FoodType != ent.Comp.FoodType)
                continue;

            var conditionsMet = true;
            foreach (var condition in recipe.Requirements)
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

    private void CookFood(Entity<CP14FoodCookerComponent> ent, CP14CookingRecipePrototype recipe)
    {
        _container.TryGetContainer(ent, ent.Comp.ContainerId, out var container);
        _solution.TryGetSolution(ent.Owner, ent.Comp.SolutionId, out var soln, out var solution);

        var newData = new CP14FoodData
        {
            Visuals = new List<PrototypeLayerData>(recipe.FoodData.Visuals),
            Trash = new List<EntProtoId>(recipe.FoodData.Trash),
            Flavors = new HashSet<LocId>(recipe.FoodData.Flavors),
            Name = recipe.FoodData.Name,
            Desc = recipe.FoodData.Desc,
            Solution = recipe.FoodData.Solution
        };

        newData.Name = recipe.FoodData.Name;
        newData.Desc = recipe.FoodData.Desc;

        //Process entities
        foreach (var contained in container?.ContainedEntities ?? [])
        {
            if (TryComp<FoodComponent>(contained, out var food))
            {
                //Merge trash
                newData.Trash.AddRange(food.Trash);

                //Merge solutions
                if (_solution.TryGetSolution(contained, food.Solution, out _, out var foodSolution))
                {
                    newData.Solution.MaxVolume += foodSolution.Volume;
                    newData.Solution.AddSolution(foodSolution, _proto);
                }
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
        Dirty(ent);
    }
}
