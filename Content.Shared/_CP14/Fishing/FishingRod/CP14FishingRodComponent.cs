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
    public float Speed = 0.1f;
}
