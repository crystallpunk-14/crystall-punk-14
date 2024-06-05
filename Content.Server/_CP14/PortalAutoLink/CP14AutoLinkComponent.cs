using Robust.Shared.Map;

namespace Content.Server._CP14.PortalAutoLink;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CP14AutoLinkSystem))]
public sealed partial class CP14AutoLinkComponent : Component
{
    /// <summary>
    ///
    /// </summary>
    [DataField]
    public string? AutoLinkKey = "DungeonLevel";
}
