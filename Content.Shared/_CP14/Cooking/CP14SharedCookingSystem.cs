/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using System.Linq;
using System.Numerics;
using Content.Shared._CP14.Cooking.Components;
using Content.Shared._CP14.Cooking.Prototypes;
using Content.Shared.Audio;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Fluids;
using Content.Shared.Nutrition.Components;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._CP14.Cooking;

public abstract partial class CP14SharedCookingSystem : EntitySystem
{
    [Dependency] protected readonly SharedContainerSystem _container = default!;
    [Dependency] protected readonly IRobustRandom _random = default!;
    [Dependency] protected readonly MetaDataSystem _metaData = default!;
    [Dependency] protected readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedPuddleSystem _puddle = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedAmbientSoundSystem _ambientSound = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    /// <summary>
    /// When overcooking food, we will replace the reagents inside with this reagent.
    /// </summary>
    private readonly ProtoId<ReagentPrototype> _burntFoodReagent = "CP14BurntFood";

    /// <summary>
    /// Stores a list of all recipes sorted by complexity: the most complex ones at the beginning.
    /// When attempting to cook, the most complex recipes will be checked first,
    /// gradually moving down to the easiest ones.
    /// The easiest recipes are usually the most “abstract,”
    /// so they will be suitable for the largest number of recipes.
    /// </summary>
    protected List<CP14CookingRecipePrototype> OrderedRecipes = [];

    public override void Initialize()
    {
        base.Initialize();
        InitTransfer();
        InitDoAfter();

        CacheAndOrderRecipes();

        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);
        SubscribeLocalEvent<CP14FoodHolderComponent, ExaminedEvent>(OnExaminedEvent);
    }

    public override void Update(float frameTime)
    {
        UpdateDoAfter(frameTime);
    }

    private void CacheAndOrderRecipes()
    {
        OrderedRecipes = _proto.EnumeratePrototypes<CP14CookingRecipePrototype>()
            .Where(recipe => recipe.Requirements.Count > 0) // Only include recipes with requirements
            .OrderByDescending(recipe => recipe.Requirements.Sum(condition => condition.GetComplexity()))
            .ToList();
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs ev)
    {
        if (!ev.WasModified<EntityPrototype>())
            return;

        CacheAndOrderRecipes();
    }

    private void OnExaminedEvent(Entity<CP14FoodHolderComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.FoodData?.Name is null)
            return;

        if (!_solution.TryGetSolution(ent.Owner, ent.Comp.SolutionId, out _, out var solution))
            return;

        if (solution.Volume == 0)
            return;

        var remaining = solution.Volume;

        args.PushMarkup(Loc.GetString("cp14-cooking-examine",
            ("name", Loc.GetString(ent.Comp.FoodData.Name)),
            ("count", remaining)));
    }


    /// <summary>
    /// Transfer food data from cooker to holder
    /// </summary>
    protected virtual bool TryTransferFood(Entity<CP14FoodHolderComponent> target, Entity<CP14FoodHolderComponent> source)
    {
        if (!source.Comp.CanGiveFood || !target.Comp.CanAcceptFood)
            return false;

        if (target.Comp.FoodType != source.Comp.FoodType)
            return false;

        if (source.Comp.FoodData is null)
            return false;

        if (!TryComp<FoodComponent>(target, out var holderFoodComp))
            return false;

        if (!_solution.TryGetSolution(source.Owner, source.Comp.SolutionId, out var cookerSoln, out var cookerSolution))
            return false;

        //Solutions
        if (_solution.TryGetSolution(target.Owner, holderFoodComp.Solution, out var holderSoln, out var solution))
        {
            if (solution.Volume > 0)
            {
                _popup.PopupEntity(Loc.GetString("cp14-cooking-popup-not-empty", ("name", MetaData(target).EntityName)),
                    target);
                return false;
            }

            _solution.TryTransferSolution(holderSoln.Value, cookerSolution, solution.MaxVolume);
        }

        //Trash
        //If we have a lot of trash, we put 1 random trash in each plate. If it's a last plate (out of solution in cooker), we put all the remaining trash in it.
        if (source.Comp.FoodData?.Trash.Count > 0)
        {
            if (cookerSolution.Volume <= 0)
            {
                holderFoodComp.Trash.AddRange(source.Comp.FoodData.Trash);
            }
            else
            {
                if (_net.IsServer)
                {
                    var newTrash = _random.Pick(source.Comp.FoodData.Trash);
                    source.Comp.FoodData.Trash.Remove(newTrash);
                    holderFoodComp.Trash.Add(newTrash);
                }
            }
        }

        if (source.Comp.FoodData is not null)
            ApplyFoodVisuals(target, source.Comp.FoodData);

        Dirty(target);
        Dirty(source);

        _solution.UpdateChemicals(cookerSoln.Value);

        return true;
    }

    private void ApplyFoodVisuals(Entity<CP14FoodHolderComponent> ent, CP14FoodData data)
    {
        //Name and Description
        if (data.Name is not null)
            _metaData.SetEntityName(ent, Loc.GetString(data.Name));
        if (data.Desc is not null)
            _metaData.SetEntityDescription(ent, Loc.GetString(data.Desc));

        //Flavors
        EnsureComp<FlavorProfileComponent>(ent, out var flavorComp);

        foreach (var flavor in data.Flavors)
        {
            flavorComp.Flavors.Add(flavor);
        }

        //Visuals
        ent.Comp.FoodData = new CP14FoodData(data);

        //Visual random
        foreach (var layer in data.Visuals)
        {
            if (_random.Prob(0.5f))
                layer.Scale = new Vector2(-1, 1);
        }
    }

    public CP14CookingRecipePrototype? GetRecipe(Entity<CP14FoodCookerComponent> ent)
    {
        if (!_container.TryGetContainer(ent, ent.Comp.ContainerId, out var container))
            return null;

        _solution.TryGetSolution(ent.Owner, ent.Comp.SolutionId, out _, out var solution);

        //Get all tags
        var allTags = new List<ProtoId<TagPrototype>>();
        foreach (var contained in container.ContainedEntities)
        {
            if (!TryComp<TagComponent>(contained, out var tags))
                continue;

            allTags.AddRange(tags.Tags);
        }

        return GetRecipe(ent.Comp.FoodType, solution, allTags);
    }

    public CP14CookingRecipePrototype? GetRecipe(ProtoId<CP14FoodTypePrototype> foodType,
        Solution? solution,
        List<ProtoId<TagPrototype>> allTags)
    {
        if (OrderedRecipes.Count == 0)
        {
            throw new InvalidOperationException(
                "No cooking recipes found. Please ensure that the CP14CookingRecipePrototype is defined and loaded.");
        }

        CP14CookingRecipePrototype? selectedRecipe = null;
        foreach (var recipe in OrderedRecipes)
        {
            if (recipe.FoodType != foodType)
                continue;

            var conditionsMet = true;
            foreach (var condition in recipe.Requirements)
            {
                if (!condition.CheckRequirement(EntityManager, _proto, allTags, solution))
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

    protected void CookFood(Entity<CP14FoodCookerComponent> ent, CP14CookingRecipePrototype recipe)
    {
        if (!_solution.TryGetSolution(ent.Owner, ent.Comp.SolutionId, out var soln, out var solution))
            return;

        if (!_container.TryGetContainer(ent, ent.Comp.ContainerId, out var container))
            return;

        var newData = new CP14FoodData(recipe.FoodData);

        //Process entities
        foreach (var contained in container.ContainedEntities)
        {
            if (TryComp<FoodComponent>(contained, out var food))
            {
                //Merge trash
                newData.Trash.AddRange(food.Trash);

                //Merge solutions
                if (_solution.TryGetSolution(contained, food.Solution, out _, out var foodSolution))
                {
                    _solution.TryMixAndOverflow(soln.Value, foodSolution, solution.MaxVolume, out var overflowed);
                    if (overflowed is not null)
                    {
                        _puddle.TrySplashSpillAt(ent, Transform(ent).Coordinates, overflowed, out _);
                    }
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

        if (solution.Volume <= 0)
            return;

        if (TryComp<CP14FoodHolderComponent>(ent.Owner, out var holder))
        {
            holder.FoodData = newData;
            Dirty(ent.Owner, holder);
        }

        Dirty(ent);
    }

    protected void BurntFood(Entity<CP14FoodCookerComponent> ent)
    {
        if (!TryComp<CP14FoodHolderComponent>(ent, out var holder) || holder.FoodData is null)
            return;

        if (!_solution.TryGetSolution(ent.Owner, ent.Comp.SolutionId, out var soln, out var solution))
            return;

        //Brown visual
        foreach (var visuals in holder.FoodData.Visuals)
        {
            visuals.Color = Color.FromHex("#212121");
        }

        holder.FoodData.Name = Loc.GetString("cp14-meal-recipe-burned-trash-name");
        holder.FoodData.Desc = Loc.GetString("cp14-meal-recipe-burned-trash-desc");

        var replacedVolume = solution.Volume / 2;
        solution.SplitSolution(replacedVolume);
        solution.AddReagent(_burntFoodReagent, replacedVolume / 2);

        DirtyField(ent.Owner, holder, nameof(CP14FoodHolderComponent.FoodData));
    }
}
