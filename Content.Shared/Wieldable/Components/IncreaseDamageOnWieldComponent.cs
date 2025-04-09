using Content.Shared.Damage;

namespace Content.Shared.Wieldable.Components;

[RegisterComponent/*, Access(typeof(SharedWieldableSystem))*/] //CP14 public access
public sealed partial class IncreaseDamageOnWieldComponent : Component
{
    [DataField("damage", required: true)]
    public DamageSpecifier BonusDamage = default!;
}
