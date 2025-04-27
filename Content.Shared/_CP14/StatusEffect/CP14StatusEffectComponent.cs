using Content.Shared.Alert;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CP14.StatusEffect;

/// <summary>
/// The base component for all status effects. Provides a link between the effect and the affected entity, and some data common to all status effects.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(CP14SharedStatusEffectSystem))]
public sealed partial class CP14StatusEffectComponent : Component
{
    /// <summary>
    /// The entity that this status effect is applied to.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? AppliedTo = null;

    /// <summary>
    /// Status effect indication for the player
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<AlertPrototype>? Alert;

    /// <summary>
    /// When this effect will end.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField, AutoNetworkedField]
    public TimeSpan? EndEffectTime;

    /// <summary>
    /// Whitelist, by which it is determined whether this status effect can be imposed on a particular entity.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist = null;

    /// <summary>
    ///
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist = null;
}
