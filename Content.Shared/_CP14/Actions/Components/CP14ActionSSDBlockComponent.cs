using Content.Shared._CP14.MagicSpell;

namespace Content.Shared._CP14.Actions.Components;

/// <summary>
/// Blocks the user from using action against target target in SSD.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem))]
public sealed partial class CP14ActionSSDBlockComponent : Component
{
}
