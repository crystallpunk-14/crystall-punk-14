namespace Content.Shared._CP14.MagicSpell.Components;

/// <summary>
/// Requires the user to have at least one free hand to use this spell
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem))]
public sealed partial class CP14MagicEffectSomaticAspectComponent : Component
{
    [DataField]
    public int FreeHandRequired = 1;
}
