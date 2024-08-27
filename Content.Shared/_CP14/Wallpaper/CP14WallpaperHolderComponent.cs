using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Wallpaper;

/// <summary>
/// Stores all wallpapers added to the wall
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), Access(typeof(CP14SharedWallpaperSystem))]
public sealed partial class CP14WallpaperHolderComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<SpriteSpecifier> Layers = new();

    public HashSet<string> RevealedLayers = new();

    [DataField]
    public int MaxLayers = 4;
}
