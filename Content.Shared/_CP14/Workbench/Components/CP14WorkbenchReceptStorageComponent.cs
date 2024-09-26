using Content.Shared._CP14.Workbench.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Workbench.Components;

/// <summary>
/// a list of recipes learned by this entity
/// </summary>
[RegisterComponent]
public sealed partial class CP14WorkbenchRecipesStorageComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public List<ProtoId<CP14WorkbenchRecipePrototype>> Recipes { get; private set; } = new();
}
