using System.Numerics;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Fishing;

[Prototype("CP14FishingMinigameStyle")]
public sealed partial class CP14FishingMinigamePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField(required: true)]
    public Dictionary<CP14FishingMinigameElement, FishingMinigameElementData>? Texture;

    [DataDefinition]
    public partial struct FishingMinigameElementData
    {
        [DataField(required: true)] public ResPath Texture;
        [DataField] public Vector2 Size;
    }
}

public enum CP14FishingMinigameElement : byte
{
    Background,
    DefaultFishIcon,
    Progressbar,
    Float,
}
