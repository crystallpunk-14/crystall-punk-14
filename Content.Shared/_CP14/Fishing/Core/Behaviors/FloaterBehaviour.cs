using Robust.Shared.Random;

namespace Content.Shared._CP14.Fishing.Core.Behaviors;

public sealed partial class FloaterBehaviour : Behavior
{
    public override float CalculateSpeed(IRobustRandom random)
    {
        return Speed * (0.5f + random.NextFloat() * Difficulty);
    }
}
