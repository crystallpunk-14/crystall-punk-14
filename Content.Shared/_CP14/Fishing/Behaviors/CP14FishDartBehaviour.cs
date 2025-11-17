using Robust.Shared.Random;

namespace Content.Shared._CP14.Fishing.Behaviors;

public sealed partial class CP14FishDartBehaviour : CP14FishBaseBehavior
{
    public override float CalculateSpeed(IRobustRandom random)
    {
        return Speed * (0.5f + random.NextFloat() * Difficulty);
    }
}
