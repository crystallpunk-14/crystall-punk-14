using Content.Shared._CP14.Workbench.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Workbench;

[Serializable, NetSerializable]
public enum CP14WorkbenchUiKey
{
    Key,
}

[Serializable, NetSerializable]
public sealed class CP14WorkbenchUiCraftMessage : BoundUserInterfaceMessage
{
    public readonly ProtoId<CP14WorkbenchRecipePrototype> Recipe;

    public CP14WorkbenchUiCraftMessage(ProtoId<CP14WorkbenchRecipePrototype> recipe)
    {
        Recipe = recipe;
    }
}


[Serializable, NetSerializable]
public sealed class CP14WorkbenchUiRecipesState : BoundUserInterfaceState
{
    // It's list (not hashset) BECAUSE CP14WorkbenchComponent contains list of recipes (WHY???)
    public readonly List<CP14WorkbenchUiRecipesEntry> Recipes;

    public CP14WorkbenchUiRecipesState(List<CP14WorkbenchUiRecipesEntry> recipes)
    {
        Recipes = recipes;
    }
}

[Serializable, NetSerializable]
public readonly record struct CP14WorkbenchUiRecipesEntry
    (ProtoId<CP14WorkbenchRecipePrototype> ProtoId, bool Craftable);
