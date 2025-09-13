using Robust.Shared.Random;

namespace Content.Shared._CP14.Fishing.Behaviors;

[ImplicitDataDefinitionForInheritors]
public abstract partial class CP14FishBaseBehavior
{
    public abstract float CalculatePosition(IRobustRandom random, float cordA);

    /// <summary>
    /// Speed which fish uses when going from A to B +- 0.2*Speed
    /// </summary>
    [DataField]
    public float Speed;

    /// <summary>
    /// How long fish will be in point B until selecting next point B +- 0.2*Difficulty
    /// </summary>
    [DataField]
    public float Difficulty;
}
