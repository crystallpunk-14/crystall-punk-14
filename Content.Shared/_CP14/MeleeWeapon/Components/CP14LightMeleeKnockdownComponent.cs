using Robust.Shared.GameStates;

namespace Content.Shared._CP14.MeleeWeapon.Components;

/// <summary>
/// After several wide attacks, a light attack deals additional damage.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CP14LightMeleeKnockdownComponent : Component
{
    [DataField]
    public float ThrowDistance = 0.5f;

    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(0.25f);
}
