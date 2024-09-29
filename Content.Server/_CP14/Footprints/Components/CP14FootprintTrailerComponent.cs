namespace Content.Server._CP14.Footprints.Components;

/// <summary>
///  allows an entity to leave footprints on the tiles
/// </summary>
[RegisterComponent, Access(typeof(CP14FootprintsSystem))]
public sealed partial class CP14FootprintTrailerComponent : Component
{
    /// <summary>
    /// Source and type of footprint
    /// </summary>
    [DataField]
    public CP14FootprintHolderComponent? holder = null;
}
