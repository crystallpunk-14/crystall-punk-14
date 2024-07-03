using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._CP14.WorldEdge;

/// <summary>
/// creates a world boundary that removes players who pass through it
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(CP14SharedWorldEdgeSystem))]
public sealed partial class CP14WorldEdgeComponent : Component
{
    [DataField(required: true), AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public float Range = 20f;

    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public Vector2 Origin;

    [DataField]
    public EntityUid BoundaryEntity;
}
