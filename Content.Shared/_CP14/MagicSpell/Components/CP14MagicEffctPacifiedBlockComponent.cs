using Content.Shared._CP14.MagicSpellStorage;
using Content.Shared.FixedPoint;

namespace Content.Shared._CP14.MagicSpell.Components;

/// <summary>
/// Blocks the target from using magic if they are pacified.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem), typeof(CP14SpellStorageSystem))]
public sealed partial class CP14MagicEffectPacifiedBlockComponent : Component
{
}
