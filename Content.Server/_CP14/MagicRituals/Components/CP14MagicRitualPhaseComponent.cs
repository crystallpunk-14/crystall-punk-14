namespace Content.Server._CP14.MagicRituals.Components;

/// <summary>
/// Magical entity that reacts to world events
/// </summary>
[RegisterComponent, Access(typeof(CP14RitualSystem))]
public sealed partial class CP14MagicRitualPhaseComponent : Component
{
    /// <summary>
    ///
    /// </summary>
    [DataField]
    public Entity<CP14MagicRitualComponent>? Ritual;
}
