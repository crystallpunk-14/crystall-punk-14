namespace Content.Shared._CP14.MagicSpell.Components;

/// <summary>
/// apply slowdown effect from casting spells
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem))]
public sealed partial class CP14MagicCasterSlowdownComponent : Component
{
    [DataField]
    public float SpeedModifier = 1f;
}
