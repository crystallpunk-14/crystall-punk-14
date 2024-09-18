/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CP14.Workbench.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared._CP14.Workbench.Components;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Workbench;

public class SharedCP14WorkbenchSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public bool TryLearnRecipe(EntityUid uid, ProtoId<CP14WorkbenchRecipePrototype> recipe)
    {
        if (!TryComp<CP14WorkbenchRecipesStorageComponent>(uid, out var recipesStorage))
        {
            _popup.PopupEntity(Loc.GetString("cp14-can-not-learn-recipe"), uid);
            return false;
        }

        if (!recipesStorage.Recipes.Contains(recipe))
        {
            recipesStorage.Recipes.Add(recipe);
            _popup.PopupEntity(Loc.GetString("cp14-recipe-has-been-learned"), uid);
            return true;
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("cp14-already-know-recipe"), uid);
            return false;
        }
    }

    public List<ProtoId<CP14WorkbenchRecipePrototype>>? GetLearnedRecipes(EntityUid uid)
    {
        if (!TryComp<CP14WorkbenchRecipesStorageComponent>(uid, out var recipesStorage))
        {
            return null;
        }

        return recipesStorage.Recipes;
    }
}

[Serializable, NetSerializable]
public sealed partial class CP14CraftDoAfterEvent : DoAfterEvent
{
    [DataField(required: true)]
    public ProtoId<CP14WorkbenchRecipePrototype> Recipe = default!;

    public override DoAfterEvent Clone() => this;
}
