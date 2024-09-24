

namespace Content.Shared._CP14.MeleeWeapon.Components;

[RegisterComponent]
public sealed partial class CP14MeleeParryComponent : Component
{
    [DataField]
    public TimeSpan ParryWindow = TimeSpan.FromSeconds(1f);

    [DataField]
    public float ParryPower = 1f;
}
