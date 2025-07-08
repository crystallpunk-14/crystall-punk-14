using Robust.Shared.Audio;

namespace Content.Shared._CP14.MeleeWeapon.Components;

/// <summary>
/// allows this item to be knocked out of your hands by a successful parry
/// </summary>
[RegisterComponent]
public sealed partial class CP14MeleeParriableComponent : Component
{
    /// <summary>
    ///
    /// </summary>
    [DataField]
    public TimeSpan LastMeleeHit = TimeSpan.Zero;

    /// <summary>
    ///
    /// </summary>
    [DataField]
    public float NeedParryPower = 1f;

    [DataField]
    public SoundSpecifier ParrySound = new SoundCollectionSpecifier("CP14Parry", AudioParams.Default.WithVariation(0.05f));
}
