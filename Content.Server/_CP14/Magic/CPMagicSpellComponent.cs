namespace Content.Server._CP14.Magic;

[RegisterComponent]
public sealed partial class CPMagicSpellComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public List<CPMagicEffectPrototype> Effects = new();
}
