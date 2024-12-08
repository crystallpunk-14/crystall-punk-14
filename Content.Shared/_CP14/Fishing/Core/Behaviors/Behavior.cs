using JetBrains.Annotations;
using Robust.Shared.Random;

namespace Content.Shared._CP14.Fishing.Core.Behaviors;

[ImplicitDataDefinitionForInheritors, MeansImplicitUse]
public abstract partial class Behavior
{
    [DataField]
    public float Speed { get; set; } = 0.05f;

    [DataField]
    public float Difficulty { get; set; } = 2f;

    public abstract float CalculateSpeed(IRobustRandom random);
}
