using Robust.Shared.Prototypes;

namespace Content.Server._CP14.DungeonPortal;

/// <summary>
/// Automatically creates a linked portal of a certain prototype on the opposite linked world, if it exists.
/// </summary>
[RegisterComponent, Access(typeof(CP14AutoDungeonPortalSystem))]
public sealed partial class CP14AutoDungeonPortalComponent : Component
{
    [DataField(required: true)]
    public EntProtoId OtherSidePortal;
}
