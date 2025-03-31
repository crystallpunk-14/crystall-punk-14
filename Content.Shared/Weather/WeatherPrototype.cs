using System.Numerics;
using Content.Shared._CP14.WeatherEffect;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Weather;

[Prototype]
public sealed partial class WeatherPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [ViewVariables(VVAccess.ReadWrite), DataField("sprite", required: true)]
    public SpriteSpecifier Sprite = default!;

    [ViewVariables(VVAccess.ReadWrite), DataField("color")]
    public Color? Color;

    /// <summary>
    /// Sound to play on the affected areas.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("sound")]
    public SoundSpecifier? Sound;

    /// <summary>
    /// CP14 offset speed
    /// </summary>
    [DataField]
    public Vector2 OffsetSpeed = Vector2.Zero;

    /// <summary>
    /// CP14 alpha
    /// </summary>
    [DataField]
    public float Alpha = 1f;

    /// <summary>
    /// CP14 Effects
    /// </summary>
    [DataField]
    public List<CP14WeatherEffectConfig> Config = new();
}
