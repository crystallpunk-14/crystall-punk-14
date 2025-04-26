using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.StatusEffect;

/// <summary>
/// A component that is automatically added to any entities that have a status effect applied to them. Allows you to track which status effects are applied to an entity right now.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
[Access(typeof(CP14SharedStatusEffectSystem))]
public sealed partial class CP14StatusEffectContainerComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<EntProtoId, EntityUid> ActiveStatusEffects = new();
}
