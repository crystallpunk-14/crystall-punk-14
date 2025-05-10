using Robust.Shared.Random;

namespace Content.Shared._CP14.Fishing.Core.Behaviors;

public sealed partial class SinkerBehavior : Behavior
{
    public override float CalculateSpeed(IRobustRandom random)
    {
        return Speed * (1.0f + random.NextFloat() * 0.5f);
    }
}
