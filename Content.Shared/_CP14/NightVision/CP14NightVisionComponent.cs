using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.NightVision;

[RegisterComponent, NetworkedComponent]
public sealed partial class CP14NightVisionComponent : Component
{
    [DataField]
    public EntityUid? LocalLightEntity = null;

    [DataField]
    public EntProtoId LightPrototype = "CP14NightVisionLight";

    [DataField]
    public EntProtoId ActionPrototype = "CP14ActionToggleNightVision";

    [DataField]
    public EntityUid? ActionEntity = null;
}
