using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Magic.Container;

[RegisterComponent]
public sealed partial class MagicSpellContainerComponent : Component
{
    [DataField]
    public List<ProtoId<MagicSpellPrototype>> Spells = new();
}
