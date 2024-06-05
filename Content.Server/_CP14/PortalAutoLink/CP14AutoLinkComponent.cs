using Robust.Shared.Map;

namespace Content.Server._CP14.PortalAutoLink;

/// <summary>
/// allows you to automatically link entities to each other, through key matching searches
/// </summary>
[RegisterComponent, Access(typeof(CP14AutoLinkSystem))]
public sealed partial class CP14AutoLinkComponent : Component
{
    /// <summary>
    /// a key that is used to search for another autolinked entity installed in the worlds
    /// </summary>
    [DataField]
    public string? AutoLinkKey = "Hello";
}
