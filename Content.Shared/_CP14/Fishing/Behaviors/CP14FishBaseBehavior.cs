using System.Numerics;
using JetBrains.Annotations;
using Robust.Shared.Random;

namespace Content.Shared._CP14.Fishing.Behaviors;

[ImplicitDataDefinitionForInheritors, UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract partial class CP14FishBaseBehavior
{
    /// <summary>
    /// Calculates fish position
    /// </summary>
    /// <param name="random"> Robust random</param>
    /// <param name="fishCords"> Current fish coordinates</param>
    /// <returns> Calculated fish coordinates </returns>
    public abstract Vector2 TryCalculatePosition(IRobustRandom random, Vector2 fishCords);

    /// <summary>
    /// Speed which fish uses when going from A to B +- 0.2 * Speed
    /// </summary>
    [DataField]
    public float Speed = 0.25f;

    /// <summary>
    /// How long fish will be in point B until selecting next point B +- 0.2 * Difficulty
    /// </summary>
    [DataField]
    public TimeSpan Difficulty = TimeSpan.FromSeconds(0.2);
}
