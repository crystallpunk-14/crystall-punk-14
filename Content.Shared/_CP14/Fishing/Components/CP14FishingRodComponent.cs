using Content.Shared._CP14.Fishing.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Fishing.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CP14FishingRodComponent : Component
{
    [ViewVariables]
    public static readonly ProtoId<CP14FishingProcessStyleSheetPrototype> DefaultStyle = "Default";

    [ViewVariables]
    public EntityUid? Process;

    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public bool Reeling;

    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float Speed = 0.05f;

    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float Gravity = 0.075f;

    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float MaxVelocity = 0.3f;

    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float MinVelocity = -0.325f;

    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float Bouncing = 0.07f;

    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float Drag = 0.98f;

    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float Size = 0.25f;

    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float ThrowPower = 10f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<CP14FishingProcessStyleSheetPrototype> Style = DefaultStyle;
}
