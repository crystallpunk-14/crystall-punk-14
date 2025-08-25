using Content.Shared._CP14.MagicSpell;

namespace Content.Shared._CP14.Action.Components;

/// <summary>
/// Blocks the target from using magic if they are pacified.
/// Also block using spell on SSD player
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem))]
public sealed partial class CP14ActionDangerousComponent : Component
{
}
