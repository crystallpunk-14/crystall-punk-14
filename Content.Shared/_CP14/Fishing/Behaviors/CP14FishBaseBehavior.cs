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
    /// <param name="random"> Robust random </param>
    /// <param name="fishCords"> Current fish coordinates </param>
    /// <returns> Calculated fish coordinates </returns>
    public Vector2 TryCalculatePosition(IRobustRandom random, Vector2 fishCords)
    {
        var speed = CalculateSpeed(random);
        var nextPos = float.Lerp(fishCords.X, fishCords.Y, speed);

        return new Vector2(nextPos, fishCords.Y);
    }

    /// <summary>
    /// Formula to calculate fish speed
    /// </summary>
    public abstract float CalculateSpeed(IRobustRandom random);

    [DataField]
    public float Speed = 0.25f;

    [DataField]
    public float Difficulty = 2f;

    [DataField]
    public TimeSpan BaseWaitTime = TimeSpan.FromSeconds(5);
}
