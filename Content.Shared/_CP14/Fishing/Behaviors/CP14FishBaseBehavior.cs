using JetBrains.Annotations;
using Robust.Shared.Random;

namespace Content.Shared._CP14.Fishing.Behaviors;

[ImplicitDataDefinitionForInheritors, UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract partial class CP14FishBaseBehavior
{
    /// <summary>
    /// Calculates fish position
    /// </summary>
    /// <param name="random"> Robust random interface </param>
    /// <param name="fishPos"> Current position of fish </param>
    /// <param name="fishDest"> Fish destination </param>
    /// <returns> Calculated fish position </returns>
    public float TryCalculatePosition(IRobustRandom random, float fishPos, float fishDest)
    {
        var speed = CalculateSpeed(random);
        var nextPos = float.Lerp(fishPos, fishDest, speed);

        return nextPos;
    }

    /// <summary>
    /// Formula to calculate fish speed
    /// </summary>
    public abstract float CalculateSpeed(IRobustRandom random);

    /// <summary>
    /// Speed of a fish
    /// </summary>
    [DataField]
    public float Speed = 0.25f;

    /// <summary>
    /// Salt of speed calculations
    /// </summary>
    [DataField]
    public float Difficulty = 2f;

    /// <summary>
    /// Base time which fish will wait in destination before selecting new
    /// </summary>
    [DataField]
    public TimeSpan BaseWaitTime = TimeSpan.FromSeconds(5);
}
