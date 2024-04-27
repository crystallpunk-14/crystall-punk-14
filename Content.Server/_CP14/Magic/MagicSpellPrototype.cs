using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Magic;

[Prototype("magicSpell")]
public sealed class MagicSpellPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = string.Empty;

    [DataField]
    public required MagicSpell Action;
}
