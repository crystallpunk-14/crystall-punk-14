using System.Numerics;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Fishing;

[Prototype("CP14FishingProcessStyleSheet")]
public sealed partial class CP14FishingProcessStyleSheetPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = string.Empty;

    [DataField, ViewVariables]
    public BackgroundData Background = new();

    [DataField, ViewVariables]
    public HandleData Handle = new();

    [DataDefinition, Serializable]
    public sealed partial class BackgroundData
    {
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public string Texture = "/Textures/_CP14/Interface/Fishing/background.png";

        [DataField("offset"), ViewVariables(VVAccess.ReadWrite)]
        public Vector2 OffsetPixels = new(10, 0);

        [DataField("handleOffset"), ViewVariables(VVAccess.ReadWrite)]
        public Vector2 HandleOffsetPixels = new(16, 4);

        [DataField("handleHeight"), ViewVariables(VVAccess.ReadWrite)]
        public float HandleHeightPixels = 149;

        [DataField("progressOffset"), ViewVariables(VVAccess.ReadWrite)]
        public Vector2 ProgressOffsetPixels = new(31, 3);

        [DataField("progressSize"), ViewVariables(VVAccess.ReadWrite)]
        public Vector2 ProgressSizePixels = new(4, 144);

        /// <remarks>
        /// In units (<c>pixels / 32</c>).
        /// </remarks>
        [ViewVariables]
        public Vector2 Offset => OffsetPixels / 32f;

        /// <remarks>
        /// In units (<c>pixels / 32</c>).
        /// </remarks>
        [ViewVariables]
        public Vector2 HandleOffset => HandleOffsetPixels / 32f;

        /// <remarks>
        /// In units (<c>pixels / 32</c>).
        /// </remarks>
        [ViewVariables]
        public Vector2 ProgressOffset => ProgressOffsetPixels / 32f;

        /// <remarks>
        /// In units (<c>pixels / 32</c>).
        /// </remarks>
        [ViewVariables]
        public Vector2 ProgressSize => ProgressSizePixels / 32f;

        /// <remarks>
        /// In units (<c>pixels / 32</c>).
        /// </remarks>
        [ViewVariables]
        public float HandleHeight => HandleHeightPixels / 32f;
    }

    [DataDefinition, Serializable]
    public sealed partial class HandleData
    {
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public string TopTexture = "/Textures/_CP14/Interface/Fishing/Handle/top.png";

        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public string MiddleTexture = "/Textures/_CP14/Interface/Fishing/Handle/middle.png";

        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public string BottomTexture = "/Textures/_CP14/Interface/Fishing/Handle/bottom.png";
    }
}
