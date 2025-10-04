using System.Numerics;
using Content.Shared._CP14.Fishing.Behaviors;
using Robust.Shared.GameStates;

namespace Content.Shared._CP14.Fishing.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class CP14FishComponent : Component
{
    [DataField(required: true), ViewVariables]
    public CP14FishBaseBehavior FishBehavior = default!;

    [AutoNetworkedField, AutoPausedField, ViewVariables]
    public TimeSpan FishSelectPosTime = TimeSpan.Zero;

    [AutoNetworkedField, AutoPausedField, ViewVariables]
    public TimeSpan FishGetAwayTime = TimeSpan.Zero;

    [AutoNetworkedField, ViewVariables]
    public Vector2 FishPosAndDestination;

    [DataField]
    public double MinAwaitTime = 1;

    [DataField]
    public double MaxAwaitTime = 4;
}
