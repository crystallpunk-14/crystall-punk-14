using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Magic;

[RegisterComponent]
public sealed partial class CPMagicSpellContainerComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public List<ProtoId<CPMagicEffectPrototype>> Effects = new();

    [DataField, ViewVariables]
    public List<CPMagicEffectPrototype> EffectPrototypes = new ();

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float MaximumCompleteness = 1f;

    [DataField, ViewVariables]
    public float TotalCompleteness;

    [DataField, ViewVariables]
    public TimeSpan TotalCastTime;
}
