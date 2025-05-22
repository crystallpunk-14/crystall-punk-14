using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Workplace;

/// <summary>
///
/// </summary>
[RegisterComponent]
public sealed partial class CP14WorkplaceRecipeComponent : Component
{
    public const string CompName = "CP14WorkplaceRecipe";

    /// <summary>
    /// Recipes matching these tags will be available for use on this workplace
    /// </summary>
    [DataField(required: true)]
    public ProtoId<TagPrototype> Tag;

    /// <summary>
    /// Recipes will be organized by category if LocId matches
    /// </summary>
    [DataField]
    public LocId? Category;
}
