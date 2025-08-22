using Content.Shared._CP14.MagicSpell;

namespace Content.Shared._CP14.Vampire.Components;

/// <summary>
/// Use is only available if the vampire is in a “visible” dangerous form.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedVampireSystem))]
public sealed partial class CP14MagicEffectVampireComponent : Component
{
}
