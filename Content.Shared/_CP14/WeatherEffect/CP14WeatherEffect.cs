using JetBrains.Annotations;
using Robust.Shared.Random;

namespace Content.Shared._CP14.WeatherEffect;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14WeatherEffect
{
    [DataField]
    public float Prob = 0.05f;

    public abstract void ApplyEffect(IEntityManager entManager, IRobustRandom random, EntityUid target);
}
