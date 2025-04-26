using Content.Shared.Alert;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CP14.StatusEffect;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
[Access(typeof(CP14SharedStatusEffectSystem))]
public sealed partial class CP14StatusEffectComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? AppliedTo = null;

    /// <summary>
    /// Status effect indication for the player
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<AlertPrototype>? Alert;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField, AutoNetworkedField]
    public TimeSpan? EndEffectTime;

    /// <summary>
    /// Whitelist, by which it is determined whether this status effect can be imposed on a particular entity.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist = null;
}
