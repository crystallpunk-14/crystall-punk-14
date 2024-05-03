using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Magic;

[RegisterComponent]
public sealed partial class CPMagicSpellContainerComponent : Component
{
    public readonly EntProtoId BaseSpellEffectEntity = "CPBaseSpellEntity";

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public List<ProtoId<CPMagicEffectPrototype>> Effects = new();

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float MaximumCompleteness = 1f;
}
