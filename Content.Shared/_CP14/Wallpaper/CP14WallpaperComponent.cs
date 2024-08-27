namespace Content.Shared._CP14.Wallpaper;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedWallpaperSystem))]
public sealed partial class CP14WallpaperComponent : Component
{
    [DataField(required: true)]
    public string RsiPath = default!;

    [DataField]
    public string Bottom = "bottom";
    [DataField]
    public string Top = "top";
    [DataField]
    public string Left = "left";
    [DataField]
    public string Right = "right";
}
