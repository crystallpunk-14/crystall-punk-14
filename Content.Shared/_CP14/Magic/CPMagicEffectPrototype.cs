using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Magic;

[Prototype("CPMagicEffect")]
public sealed partial class CPMagicEffectPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = string.Empty;

    [DataField]
    public ComponentRegistry Components = new();

    [DataField]
    public float Complexity = 1f;

    [DataField]
    public TimeSpan CastTime = TimeSpan.Zero;
}
