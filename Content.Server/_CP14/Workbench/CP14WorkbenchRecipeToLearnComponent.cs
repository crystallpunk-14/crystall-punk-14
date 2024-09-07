using Content.Shared._CP14.Workbench.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Workbench;


[RegisterComponent]
public sealed partial class CP14WorkbenchRecipeToLearnComponent : Component
{
    /// <summary>
    /// Recipe id to learn.
    /// </summary>
    [DataField(required:true)]
    public ProtoId<CP14WorkbenchRecipePrototype> Recipe = new();
}
