using Robust.Shared.Random;

namespace Content.Shared._CP14.Fishing.Behaviors;

[ImplicitDataDefinitionForInheritors]
public abstract partial class CP14FishBaseBehavior
{
    public abstract void CalculatePosition(IRobustRandom random);

    [DataField]
    public float Speed;

    [DataField]
    public float Difficulty;
}
