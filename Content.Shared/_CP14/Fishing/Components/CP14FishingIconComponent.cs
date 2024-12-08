using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Fishing.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class CP14FishingIconComponent : Component
{
    public static readonly ResPath DefaultTexturePath = new("/Textures/_CP14/Interface/Fishing/Icons/default.png");

    [DataField]
    public ResPath TexturePath = DefaultTexturePath;
}
