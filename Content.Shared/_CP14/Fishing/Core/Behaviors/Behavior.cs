using JetBrains.Annotations;
using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Fishing.Core.Behaviors;

[ImplicitDataDefinitionForInheritors, MeansImplicitUse]
[Serializable, NetSerializable]
public abstract partial class Behavior
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Speed { get; set; } = 0.25f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Difficulty { get; set; } = 2f;

    public abstract float CalculateSpeed(IRobustRandom random);

    public virtual TimeSpan CalculateDelay(IRobustRandom random)
    {
        return TimeSpan.FromSeconds(random.NextFloat(1.5f - 1f / Difficulty, 2.5f - 1f / Difficulty));
    }
}
