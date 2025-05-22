using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Workplace;

/// <summary>
///
/// </summary>
[RegisterComponent]
public sealed partial class CP14WorkplaceComponent : Component
{
    /// <summary>
    /// Recipes matching these tags will be available for use on this workplace
    /// </summary>
    [DataField]
    public HashSet<ProtoId<TagPrototype>> Tags = new();

    /// <summary>
    /// The cached list of recipes available for crafting at this workstation.
    /// Cached when initializing a workstation or reloading prototypes
    /// </summary>
    [DataField]
    public HashSet<EntProtoId> CachedRecipes = new();
}
