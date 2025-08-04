namespace Content.Server._CP14.Procedural.GlobalWorld.Components;

/// <summary>
/// The GlobalWorld system connects worlds by creating connecting portals at the location of these markers.
/// </summary>
[RegisterComponent, Access(typeof(CP14GlobalWorldSystem))]
public sealed partial class CP14GlobalWorldConnectorComponent : Component
{
}
