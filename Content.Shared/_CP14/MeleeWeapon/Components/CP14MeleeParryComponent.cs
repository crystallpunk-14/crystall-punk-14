namespace Content.Shared._CP14.MeleeWeapon.Components;

/// <summary>
/// attacks with this item may knock CP14ParriableComponent items out of your hand on a hit
/// </summary>
[RegisterComponent]
public sealed partial class CP14MeleeParryComponent : Component
{
    [DataField]
    public TimeSpan ParryWindow = TimeSpan.FromSeconds(1f);

    [DataField]
    public float ParryPower = 1f;
}
