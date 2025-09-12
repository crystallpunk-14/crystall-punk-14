using Content.Shared._CP14.Fishing.Behaviors;

namespace Content.Shared._CP14.Fishing.Components;

[RegisterComponent]
public sealed partial class CP14FishComponent : Component
{
    [DataField(required: true)]
    public CP14FishBaseBehavior FishBehavior;

    [DataField]
    public double MinAwaitTime = 1;

    [DataField]
    public double MaxAwaitTime = 4;
}
