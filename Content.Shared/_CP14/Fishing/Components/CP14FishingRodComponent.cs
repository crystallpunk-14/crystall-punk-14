using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Fishing.Components;

/// <summary>
/// Allows to fish with this item
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class CP14FishingRodComponent : Component
{
    // Vars
    [AutoNetworkedField, ViewVariables]
    public EntityUid? FishingFloat;

    [AutoNetworkedField, ViewVariables]
    public EntityUid? User;

    [AutoNetworkedField, ViewVariables]
    public float FloatPosition = 0f;

    [AutoNetworkedField, AutoPausedField, ViewVariables]
    public TimeSpan FishingTime = TimeSpan.Zero;

    [AutoNetworkedField, ViewVariables]
    public EntityUid? CaughtFish;

    [AutoNetworkedField, ViewVariables]
    public bool Reeling;

    [AutoNetworkedField, ViewVariables]
    public bool FishHooked;

    [AutoNetworkedField, ViewVariables]
    public EntityUid? Target;

    // Data definitions
    [DataField]
    public EntProtoId FloatPrototype = "CP14DefaultFishingFloat";

    [DataField]
    public ProtoId<CP14FishingMinigamePrototype> FishingMinigame = "Default";

    [DataField]
    public float FloatSpeed = 2f;

    [DataField]
    public float MaxFishingDistance = 5f;

    [DataField]
    public float ThrowPower = 10f;

    [DataField]
    public double MinAwaitTime = 5;

    [DataField]
    public double MaxAwaitTime = 20;
}
