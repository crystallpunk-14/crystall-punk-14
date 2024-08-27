namespace Content.Shared._CP14.Wallpaper;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedWallpaperSystem))]
public sealed partial class CP14WallpaperRemoverComponent : Component
{
    [DataField]
    public float Delay = 1f;
}
