using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._CP14.DayCycle;

/// <summary>
/// if added to the map, renders cloud shadows on the map
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CP14CloudShadowsComponent : Component
{
    [DataField]
    public Vector2 CloudSpeed = new Vector2(0.5f, 0f);

    [DataField]
    public float Alpha = 1f;
}
