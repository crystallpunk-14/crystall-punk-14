using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._CP14.DayCycle;

/// <summary>
/// if added to the map, renders cloud shadows on the map
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CP14CloudShadowsComponent : Component
{
    [DataField, AutoNetworkedField]
    public Vector2 CloudSpeed = new Vector2(0.5f, 0f);

    [DataField]
    public float MaxSpeed = 1.5f;

    [DataField, AutoNetworkedField]
    public float Alpha = 1f;

    [DataField]
    public float Scale = 2.5f;

    [DataField]
    public string ParallaxPath = "/Textures/_CP14/Parallaxes/Shadows.png";
}
