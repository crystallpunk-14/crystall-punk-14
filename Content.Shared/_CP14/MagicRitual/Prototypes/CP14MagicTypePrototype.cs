using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitual.Prototypes;

/// <summary>
///     A round-start setup preset, such as which antagonists to spawn.
/// </summary>
[Prototype("magicType")]
public sealed partial class CP14MagicTypePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public string Name = string.Empty;

    [DataField(required: true)]
    public Color Color = Color.White;
}
