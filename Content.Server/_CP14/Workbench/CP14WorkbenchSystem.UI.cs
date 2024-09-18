/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CP14.Workbench;
using Content.Shared._CP14.Workbench.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Workbench;

public sealed partial class CP14WorkbenchSystem
{
    [Dependency] private readonly SharedCP14WorkbenchSystem _sharedWorkbenchSystem = default!;
    private void OnCraft(Entity<CP14WorkbenchComponent> entity, ref CP14WorkbenchUiCraftMessage args)
    {
        var recipes = new List<ProtoId<CP14WorkbenchRecipePrototype>>();

        foreach(var recipe in entity.Comp.Recipes)
            recipes.Add(recipe);

        var userLearnedRecipes = _sharedWorkbenchSystem.GetLearnedRecipes(args.Actor);
        if (userLearnedRecipes is null)
            return;
        foreach(var recipe in userLearnedRecipes)
        {
            if (!recipes.Contains(recipe) && entity.Comp.SecretRecipes.Contains(recipe))
                recipes.Add(recipe);
        }

        if (!recipes.Contains(args.Recipe))
            return;

        if (!_proto.TryIndex(args.Recipe, out var prototype))
            return;

        StartCraft(entity, args.Actor, prototype);
    }

    private void UpdateUIRecipes(Entity<CP14WorkbenchComponent> entity, EntityUid user)
    {
        var placedEntities = _lookup.GetEntitiesInRange(Transform(entity).Coordinates, WorkbenchRadius);
        var recipes = new List<CP14WorkbenchUiRecipesEntry>();

        var userLearnedRecipes = _sharedWorkbenchSystem.GetLearnedRecipes(user);
        if (userLearnedRecipes is null)
            return;
        foreach(var learnedRecipe in userLearnedRecipes)
        {
            if (!entity.Comp.Recipes.Contains(learnedRecipe) && entity.Comp.SecretRecipes.Contains(learnedRecipe))
            {
                var recipe = _proto.Index(learnedRecipe);
                var entry = new CP14WorkbenchUiRecipesEntry(learnedRecipe, CanCraftRecipe(recipe, placedEntities));

                recipes.Add(entry);
            }
        }

        foreach (var recipeId in entity.Comp.Recipes)
        {
            var recipe = _proto.Index(recipeId);
            var entry = new CP14WorkbenchUiRecipesEntry(recipeId, CanCraftRecipe(recipe, placedEntities));

            if (!recipes.Contains(entry))
                recipes.Add(entry);
        }

        _userInterface.SetUiState(entity.Owner, CP14WorkbenchUiKey.Key, new CP14WorkbenchUiRecipesState(recipes));
    }
}
