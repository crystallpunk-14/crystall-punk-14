using Robust.Shared.Map;

namespace Content.Server._CP14.PortalAutoLink;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CPAutoLinkSystem))]
public sealed partial class CPAutoLinkComponent : Component
{
    /// <summary>
    ///
    /// </summary>
    [DataField]
    public string? AutoLinkKey = "DungeonLevel";
}
