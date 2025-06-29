namespace Content.Shared._CP14.MagicSpell.Components;

/// <summary>
/// Restricts the use of this action, by spending stamina.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem))]
public sealed partial class CP14MagicEffectStaminaCostComponent : Component
{
    [DataField]
    public float Stamina = 0f;
}
