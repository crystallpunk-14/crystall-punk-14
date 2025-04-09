namespace Content.Shared._CP14.MagicSpell.Components;

/// <summary>
/// Requires the user to play music to use this spell
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem))]
public sealed partial class CP14MagicEffectRequiredMusicToolComponent : Component
{
}
