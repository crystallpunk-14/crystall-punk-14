using System.Numerics;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Fishing;

/// <summary>
/// Prototype of fishing minigame stylesheet.
/// </summary>
[Prototype("CP14FishingMinigameStyle")]
public sealed partial class CP14FishingMinigamePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField(required: true)]
    public FishingMinigameElementData Background;

    [DataField(required: true)]
    public FishingMinigameElementData FishIcon;

    [DataField(required: true)]
    public FishingMinigameElementData Progressbar;

    [DataField(required: true)]
    public FishingMinigameElementData Float;

    [DataDefinition]
    public partial struct FishingMinigameElementData
    {
        /// <summary>
        /// Texture path.
        /// </summary>
        [DataField(required: true)] public ResPath Texture;

        /// <summary>
        /// Size of a texture.
        /// </summary>
        [DataField(required: true)] public Vector2 Size;

        /// <summary>
        /// Offset from top left corner. Starter position in the bottom.
        /// </summary>
        [DataField(required: true)] public Vector2 Offset;
    }
}
