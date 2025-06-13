using Content.Shared._CP14.MagicSpellStorage;
using Content.Shared.FixedPoint;

namespace Content.Shared._CP14.MagicSpell.Components;

/// <summary>
/// If the user belongs to a religion, this spell can only be used within the area of influence of that religion
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem), typeof(CP14SpellStorageSystem))]
public sealed partial class CP14MagicEffectReligionRestrictedComponent : Component
{
}
