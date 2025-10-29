using System.Numerics;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Fishing;

/// <summary>
/// Prototype of fishing minigame. Starter position of minigame in the bottom
/// </summary>
[Prototype("CP14FishingMinigameStyle")]
public sealed class CP14FishingMinigamePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    /// Fishing minigame background data
    /// </summary>
    [DataField(required: true)]
    public FishingMinigameElementData Background;

    /// <summary>
    /// Fishing minigame fish icon data
    /// </summary>
    [DataField(required: true)]
    public FishingMinigameElementData FishIcon;

    /// <summary>
    /// Fishing minigame progressbar data
    /// </summary>
    [DataField(required: true)]
    public FishingMinigameElementData Progressbar;

    /// <summary>
    /// Fishing minigame float data
    /// </summary>
    [DataField(required: true)]
    public FishingMinigameElementData Float;

    /// <summary>
    /// Size of the area where the float and fish will move
    /// </summary>
    [DataField(required: true)]
    public float FishingMinigameSize;
}

[DataDefinition]
public partial struct FishingMinigameElementData
{
    /// <summary>
    /// Texture path
    /// </summary>
    [DataField(required: true)] public ResPath Texture;

    /// <summary>
    /// Size of a texture
    /// </summary>
    [DataField(required: true)] public Vector2 Size;

    /// <summary>
    /// Offset from bottom left corner
    /// </summary>
    [DataField(required: true)] public Vector2 Offset;
}
