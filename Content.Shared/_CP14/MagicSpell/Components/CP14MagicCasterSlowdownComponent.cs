namespace Content.Shared._CP14.MagicSpell.Components;

/// <summary>
/// imposes debuffs on excessive use of magic
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem))]
public sealed partial class CP14MagicCasterSlowdownComponent : Component
{
    [DataField]
    public List<float> SpeedModifiers = new();
}
