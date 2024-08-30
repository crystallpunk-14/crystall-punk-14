namespace Content.Shared._CP14.Wallpaper;

/// <summary>
/// After a delay, it removes all wallpaper from the entity.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedWallpaperSystem))]
public sealed partial class CP14WallpaperRemoverComponent : Component
{
    [DataField]
    public float Delay = 1f;
}
