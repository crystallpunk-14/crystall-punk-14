using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Fishing.Components;

/// <summary>
/// Allows to fish with this item
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class CP14FishingRodComponent : Component
{
    [AutoNetworkedField]
    public EntityUid? FishingFloat;

    [AutoNetworkedField]
    public EntityUid? Target;

    [AutoNetworkedField, AutoPausedField]
    public TimeSpan FishingTime = TimeSpan.Zero;

    [AutoNetworkedField]
    public bool FishCaught;

    [DataField]
    public EntProtoId FloatPrototype = "CP14DefaultFishingFloat";

    [DataField]
    public ProtoId<CP14FishingMinigamePrototype> FishingMinigame = "Default";

    [DataField]
    public float MaxFishingDistance = 5f;

    [DataField]
    public float ThrowPower = 10f;

    [DataField]
    public double MinAwaitTime = 5;

    [DataField]
    public double MaxAwaitTime = 20;
}
