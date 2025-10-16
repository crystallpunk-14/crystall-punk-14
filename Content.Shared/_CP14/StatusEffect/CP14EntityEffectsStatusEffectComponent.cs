using Content.Shared.EntityEffects;
using Robust.Shared.GameStates;

namespace Content.Shared._CP14.StatusEffect;

/// <summary>
/// Applies Entity Effects at a given frequency
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]

public sealed partial class CP14EntityEffectsStatusEffectComponent : Component
{

    /// <summary>
    /// List of Effects that will be applied
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<EntityEffect> Effects = [];

    /// <summary>
    /// how often objects will try to apply <see cref="Effects"/>. In Seconds.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan Frequency = TimeSpan.FromSeconds(5);

    /// <summary>
    /// the time of the next Effect trigger
    /// </summary>
    [DataField]
    public TimeSpan NextUpdateTime { get; set; } = TimeSpan.Zero;
}
