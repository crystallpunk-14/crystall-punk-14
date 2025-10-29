using System.Numerics;
using Content.Shared._CP14.Fishing.Behaviors;
using Robust.Shared.GameStates;

namespace Content.Shared._CP14.Fishing.Components;

/// <summary>
/// Component for fish, that can be caught via fishing
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause, Access(typeof(CP14SharedFishingSystem))]
public sealed partial class CP14FishComponent : Component
{
    /// <summary>
    /// Fish behaviour that will be used for speed calculations
    /// </summary>
    [DataField(required: true), ViewVariables]
    public CP14FishBaseBehavior Behavior;

    /// <summary>
    /// Time when fish will select next position
    /// </summary>
    [AutoNetworkedField, AutoPausedField, ViewVariables]
    public TimeSpan SelectPosTime = TimeSpan.Zero;

    /// <summary>
    /// Time when the fish will get away if it is not hooked
    /// </summary>
    [AutoNetworkedField, AutoPausedField, ViewVariables]
    public TimeSpan GetAwayTime = TimeSpan.Zero;

    /// <summary>
    /// Fish current position in minigame coordinates
    /// </summary>
    [AutoNetworkedField, ViewVariables]
    public float Position;

    /// <summary>
    /// Fish destination in minigame coordinates
    /// </summary>
    [AutoNetworkedField, ViewVariables]
    public float Destination;

    /// <summary>
    /// Minimal time before fish will get away
    /// </summary>
    [DataField]
    public double MinAwaitTime = 5;

    /// <summary>
    /// Maximum time before fish will get away
    /// </summary>
    [DataField]
    public double MaxAwaitTime = 10;
}
