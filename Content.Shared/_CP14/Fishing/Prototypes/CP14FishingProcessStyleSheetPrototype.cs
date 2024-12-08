using System.Numerics;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Fishing.Prototypes;

/// <summary>
/// Represents the style sheet for the fishing process, containing UI elements like background and handle configurations.
/// </summary>
[Prototype("CP14FishingProcessStyleSheet")]
public sealed partial class CP14FishingProcessStyleSheetPrototype : IPrototype
{
    /// <summary>
    /// Gets the unique identifier for this fishing process style sheet prototype.
    /// </summary>
    [IdDataField]
    public string ID { get; } = string.Empty;

    /// <summary>
    /// Background settings for the fishing process UI.
    /// </summary>
    [DataField, ViewVariables]
    public BackgroundData Background = new();

    /// <summary>
    /// Handle settings for the fishing process UI.
    /// </summary>
    [DataField, ViewVariables]
    public HandleData Handle = new();

    /// <summary>
    /// Contains data related to the background of the fishing process UI.
    /// </summary>
    [DataDefinition, Serializable]
    public sealed partial class BackgroundData
    {
        /// <summary>
        /// Path to the background texture image.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public string Texture = "/Textures/_CP14/Interface/Fishing/background.png";

        /// <summary>
        /// Offset of the background texture in pixels.
        /// </summary>
        [DataField("offset"), ViewVariables(VVAccess.ReadWrite)]
        public Vector2 OffsetPixels = new(10, 0);

        /// <summary>
        /// Offset of the handle in pixels.
        /// </summary>
        [DataField("handleOffset"), ViewVariables(VVAccess.ReadWrite)]
        public Vector2 HandleOffsetPixels = new(16, 4);

        /// <summary>
        /// Size of the handle in pixels.
        /// </summary>
        [DataField("handleSize"), ViewVariables(VVAccess.ReadWrite)]
        public Vector2 HandleSizePixels = new(149, 0);

        /// <summary>
        /// Offset of the progress bar in pixels.
        /// </summary>
        [DataField("progressOffset"), ViewVariables(VVAccess.ReadWrite)]
        public Vector2 ProgressOffsetPixels = new(31, 3);

        /// <summary>
        /// Size of the progress bar in pixels.
        /// </summary>
        [DataField("progressSize"), ViewVariables(VVAccess.ReadWrite)]
        public Vector2 ProgressSizePixels = new(4, 144);

        /// <summary>
        /// Gets the background offset in units (pixels divided by 32).
        /// </summary>
        [ViewVariables]
        public Vector2 Offset => OffsetPixels / 32f;

        /// <summary>
        /// Gets the handle offset in units (pixels divided by 32).
        /// </summary>
        [ViewVariables]
        public Vector2 HandleOffset => HandleOffsetPixels / 32f;

        /// <summary>
        /// Gets the progress bar offset in units (pixels divided by 32).
        /// </summary>
        [ViewVariables]
        public Vector2 ProgressOffset => ProgressOffsetPixels / 32f;

        /// <summary>
        /// Gets the progress bar size in units (pixels divided by 32).
        /// </summary>
        [ViewVariables]
        public Vector2 ProgressSize => ProgressSizePixels / 32f;

        /// <summary>
        /// Gets the handle size in units (pixels divided by 32).
        /// </summary>
        [ViewVariables]
        public Vector2 HandleSize => HandleSizePixels / 32f;
    }

    /// <summary>
    /// Contains data related to the handle elements of the fishing process UI.
    /// </summary>
    [DataDefinition, Serializable]
    public sealed partial class HandleData
    {
        /// <summary>
        /// Path to the texture for the top part of the handle.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public string TopTexture = "/Textures/_CP14/Interface/Fishing/Handle/top.png";

        /// <summary>
        /// Path to the texture for the middle part of the handle.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public string MiddleTexture = "/Textures/_CP14/Interface/Fishing/Handle/middle.png";

        /// <summary>
        /// Path to the texture for the bottom part of the handle.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public string BottomTexture = "/Textures/_CP14/Interface/Fishing/Handle/bottom.png";
    }
}
