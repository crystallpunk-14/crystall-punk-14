/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CP14.Workbench;

namespace Content.Server._CP14.Workbench;

public sealed partial class CP14WorkbenchSystem
{
    private void OnCraft(Entity<CP14WorkbenchComponent> entity, ref CP14WorkbenchUiCraftMessage args)
    {
        if (!entity.Comp.Recipes.Contains(args.Recipe))
            return;

        if (!_proto.TryIndex(args.Recipe, out var prototype))
            return;

        StartCraft(entity, args.Actor, prototype);
    }

    private void UpdateUIRecipes(Entity<CP14WorkbenchComponent> entity, EntityUid user)
    {
        var placedEntities = _lookup.GetEntitiesInRange(Transform(entity).Coordinates, entity.Comp.WorkbenchRadius);

        var recipes = new List<CP14WorkbenchUiRecipesEntry>();
        foreach (var recipeId in entity.Comp.Recipes)
        {
            if (!_proto.TryIndex(recipeId, out var indexedRecipe))
                continue;

            if (indexedRecipe.KnowledgeRequired is not null)
            {
                if (!_knowledge.HasKnowledge(user, indexedRecipe.KnowledgeRequired.Value))
                    continue;
            }

            var entry = new CP14WorkbenchUiRecipesEntry(recipeId, CanCraftRecipe(indexedRecipe, placedEntities, user));

            recipes.Add(entry);
        }

        _userInterface.SetUiState(entity.Owner, CP14WorkbenchUiKey.Key, new CP14WorkbenchUiRecipesState(recipes));
    }
}
