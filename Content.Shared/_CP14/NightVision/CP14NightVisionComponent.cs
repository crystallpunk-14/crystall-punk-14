using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.NightVision;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CP14NightVisionComponent : Component
{
    [DataField]
    public EntityUid? LocalLightEntity = null;

    [DataField, AutoNetworkedField]
    public EntProtoId LightPrototype = "CP14NightVisionLight";

    [DataField, AutoNetworkedField]
    public EntProtoId ActionPrototype = "CP14ActionToggleNightVision";

    [DataField]
    public EntityUid? ActionEntity = null;
}
