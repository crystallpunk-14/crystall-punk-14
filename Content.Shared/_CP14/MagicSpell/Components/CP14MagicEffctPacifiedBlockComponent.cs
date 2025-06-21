namespace Content.Shared._CP14.MagicSpell.Components;

/// <summary>
/// Blocks the target from using magic if they are pacified.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem))]
public sealed partial class CP14MagicEffectPacifiedBlockComponent : Component
{
}
