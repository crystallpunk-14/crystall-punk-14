using Robust.Shared.GameStates;

namespace Content.Shared._CP14.Fishing.FishingRod;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CP14FishingRodComponent : Component
{
    [ViewVariables]
    public EntityUid? Process;

    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public bool Reeling;

    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float Speed = 0.5f;

    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float Gravity = 0.75f;

    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float MaxVelocity = 3f;

    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float MinVelocity = -3.25f;

    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float Bouncing = 0.7f;
}
