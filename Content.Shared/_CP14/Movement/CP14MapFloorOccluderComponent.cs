using Robust.Shared.GameStates;

namespace Content.Shared.Movement.Components;

/// <summary>
/// Applies floor occlusion to any <see cref="FloorOcclusionComponent"/> that reparent to this map entity.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CP14MapFloorOccluderComponent : Component
{

}
