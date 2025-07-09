using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Wounds;

/// <summary>
/// This status effect can be compounded or alleviated by moving into other stages
/// Use only in conjunction with <see cref="StatusEffectComponent"/>, on the status effect entity.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CP14ProgressiveStatusEffectComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public CP14WoundSeverity Severity = CP14WoundSeverity.Minor;

    /// <summary>
    /// Possible complications of this wound (the current status effect will be replaced with a more dangerous one). If null, the status effect will no longer be worsened
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<EntProtoId> Complications = new();

    /// <summary>
    /// Possible wound regeneration (the current status effect will be replaced with a less dangerous one). If empty, the status effect is completely lost
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<EntProtoId> Restorations = new();

    /// <summary>
    /// Current wound health. If health drops to 0, the wound heals and becomes 1 stage easier.
    /// If health rises to maximum, the wound becomes more dangerous, advancing to the next stage.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float WoundHealth = 50f;

    [DataField, AutoNetworkedField]
    public float WoundMaxHealth = 100f;

    /// <summary>
    /// Gradual change in wound health per second
    /// </summary>
    [DataField, AutoNetworkedField]
    public float WoundHealthRegen = 0.5f;

    [DataField]
    public TimeSpan NextUpdateTime = TimeSpan.Zero;
}

/// <summary>
/// Types of wound complexity. Complications should always move the wound up one stage, while healing should move it down one stage.
/// </summary>
public enum CP14WoundSeverity : byte
{
    Minor,
    Moderate,
    Severe
}
