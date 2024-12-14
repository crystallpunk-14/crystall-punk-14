using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Fishing.Core.Behaviors;

[Serializable, NetSerializable]
public sealed partial class MixedBehavior : Behavior
{
    public override float CalculateSpeed(IRobustRandom random)
    {
        return Speed * (random.NextFloat() - 0.5f);
    }
}
