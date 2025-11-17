using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Fishing.Components;

/// <summary>
/// Allows to fish with this item
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true, true), AutoGenerateComponentPause, Access(typeof(CP14SharedFishingSystem))]
public sealed partial class CP14FishingRodComponent : Component
{
    // Vars

    /// <summary>
    /// Link to a fishing float, attached to rod
    /// </summary>
    [AutoNetworkedField, ViewVariables]
    public EntityUid? FishingFloat;

    /// <summary>
    /// Link to fishing rod user
    /// </summary>
    [AutoNetworkedField, ViewVariables]
    public EntityUid? User;

    /// <summary>
    /// Link to caught fish
    /// </summary>
    [AutoNetworkedField, ViewVariables]
    public EntityUid? CaughtFish;

    /// <summary>
    /// Link to a target fishing pond
    /// </summary>
    [AutoNetworkedField, ViewVariables]
    public EntityUid? Target;

    /// <summary>
    /// Float position in minigame coordinates
    /// </summary>
    [AutoNetworkedField, ViewVariables]
    public float FloatPosition = 0f;

    /// <summary>
    /// Time when fish will be caught
    /// </summary>
    [AutoNetworkedField, AutoPausedField, ViewVariables]
    public TimeSpan FishingTime = TimeSpan.Zero;

    /// <summary>
    /// Does the user pull the fishing line
    /// </summary>
    [AutoNetworkedField, ViewVariables]
    public bool Reeling;

    /// <summary>
    /// Is fish hooked
    /// </summary>
    [AutoNetworkedField, ViewVariables]
    public bool FishHooked;

    // Data definitions

    /// <summary>
    /// Fishing float prototype
    /// </summary>
    [DataField]
    public EntProtoId FloatPrototype = "CP14DefaultFishingFloat";

    /// <summary>
    /// Fishing minigame prototype
    /// </summary>
    [DataField]
    public ProtoId<CP14FishingMinigamePrototype> FishingMinigame = "Default";

    /// <summary>
    /// Speed of a float in minigame coordinates
    /// </summary>
    [DataField]
    public float FloatSpeed = 2f;

    /// <summary>
    /// Max distance between rod and float
    /// </summary>
    [DataField]
    public float MaxFishingDistance = 5f;

    /// <summary>
    /// Power with which float will be thrown
    /// </summary>
    [DataField]
    public float ThrowPower = 10f;

    /// <summary>
    /// Minimal time before fish will be caught
    /// </summary>
    [DataField]
    public double MinAwaitTime = 5;

    /// <summary>
    /// Maximum time before fish will be caught
    /// </summary>
    [DataField]
    public double MaxAwaitTime = 20;
}
