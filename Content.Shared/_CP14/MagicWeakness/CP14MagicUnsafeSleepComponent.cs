using Content.Shared.Damage;
using Content.Shared.FixedPoint;

namespace Content.Shared._CP14.MagicWeakness;

/// <summary>
/// imposes debuffs on excessive use of magic
/// </summary>
[RegisterComponent, Access(typeof(CP14MagicWeaknessSystem))]
public sealed partial class CP14MagicUnsafeSleepComponent : Component
{
    [DataField]
    public float SleepPerEnergy = 0.5f;

    /// <summary>
    /// At the specified amount of extra mana expenditure, the character falls asleep.
    /// </summary>
    [DataField]
    public FixedPoint2 SleepThreshold = 20f;
}
