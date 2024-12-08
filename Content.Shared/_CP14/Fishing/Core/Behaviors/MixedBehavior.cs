using Robust.Shared.Random;

namespace Content.Shared._CP14.Fishing.Core.Behaviors;

public sealed partial class MixedBehavior : Behavior
{
    public override float CalculateSpeed(IRobustRandom random)
    {
        return Speed * (random.NextFloat() - 0.5f);
    }
}
