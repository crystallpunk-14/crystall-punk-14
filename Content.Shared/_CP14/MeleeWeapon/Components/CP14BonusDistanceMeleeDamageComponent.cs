using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MeleeWeapon.Components;

/// <summary>
/// Adds bonus damage to weapons if targets are at a certain distance from the attacker.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CP14BonusDistanceMeleeDamageComponent : Component
{
    [DataField]
    public DamageSpecifier BonusDamage = new();

    [DataField]
    public float MinDistance = 1f;

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_CP14/Effects/critical.ogg")
    {
        Params = AudioParams.Default.WithVariation(0.125f),
    };

    [DataField]
    public EntProtoId VFX = "CP14MeleeCritEffect";
}
