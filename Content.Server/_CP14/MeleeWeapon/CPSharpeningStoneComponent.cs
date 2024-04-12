using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Server._CP14.MeleeWeapon;

/// <summary>
/// component allows you to sharpen objects by restoring their damage.
/// </summary>
[RegisterComponent, Access(typeof(CPSharpeningSystem))]
public sealed partial class CPSharpeningStoneComponent : Component
{
    /// <summary>
    /// the amount of acuity recoverable per use
    /// </summary>
    [DataField]
    public float SharpnessHeal = 0.05f;

    /// <summary>
    /// sound when used
    /// </summary>
    [DataField]
    public SoundSpecifier SharpeningSound =
        new SoundPathSpecifier("/Audio/_CP14/Items/sharpening_stone.ogg")
    {
        Params = AudioParams.Default.WithVariation(0.02f),
    };

    /// <summary>
    /// the damage that the sharpening stone does to itself for use
    /// </summary>
    [DataField]
    public DamageSpecifier SelfDamage = new()
    {
        DamageDict = new()
        {
            { "Blunt", 1 }
        }
    };

    /// <summary>
    /// the damage the sharpening stone does to the target
    /// </summary>
    [DataField]
    public DamageSpecifier TargetDamage = new()
    {
        DamageDict = new()
        {
            { "Blunt", 1 }
        }
    };
}
