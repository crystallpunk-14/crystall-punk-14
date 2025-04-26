using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.StatusEffect;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
[Access(typeof(CP14SharedStatusEffectSystem))]
public sealed partial class CP14StatusEffectContainerComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<EntProtoId, EntityUid> ActiveStatusEffects = new();
}
