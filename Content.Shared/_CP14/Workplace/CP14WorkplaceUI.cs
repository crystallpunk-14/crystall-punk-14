using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Workplace;

[Serializable, NetSerializable]
public enum CP14WorkplaceUiKey
{
    Key,
}

[Serializable, NetSerializable]
public sealed class CP14WorkplaceCraftMessage(EntProtoId recipe) : BoundUserInterfaceMessage
{
    public readonly EntProtoId Recipe = recipe;
}

[Serializable, NetSerializable]
public sealed class CP14WorkplaceState(NetEntity workplace, NetEntity user, List<CP14WorkplaceRecipeEntry> recipes) : BoundUserInterfaceState
{
    public readonly NetEntity User = user;
    public readonly NetEntity Workplace = workplace;
    public readonly List<CP14WorkplaceRecipeEntry> Recipes = recipes;
}

[Serializable, NetSerializable]
public readonly record struct CP14WorkplaceRecipeEntry(
    EntProtoId Recipe
);
