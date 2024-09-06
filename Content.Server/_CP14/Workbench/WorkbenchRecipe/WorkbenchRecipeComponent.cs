using Content.Shared._CP14.Workbench.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Workbench.WorkbenchRecipe;


[RegisterComponent]
[Access(typeof(CP14WorkbenchRecipeSystem))]
public sealed partial class CP14WorkbenchRecipeComponent : Component
{
    /// <summary>
    /// Recipe id, to add to workbench.
    /// </summary>
    [DataField]
    public ProtoId<CP14WorkbenchRecipePrototype> Recipe = new();
}
