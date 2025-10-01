namespace Content.Shared._CP14.Actions.Components;

/// <summary>
/// Slows the caster while using action
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedActionSystem))]
public sealed partial class CP14ActionDoAfterSlowdownComponent : Component
{
    [DataField]
    public float SpeedMultiplier = 1f;
}
