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

[DataDefinition]
public sealed partial class CP14WeatherEffectConfig
{
    [DataField]
    public List<CP14WeatherEffect> Effects = new();

    [DataField]
    public int? MaxEntities = null;

    [DataField]
    public TimeSpan Frequency = TimeSpan.FromSeconds(5f);

    [DataField]
    public TimeSpan NextEffectTime = TimeSpan.Zero;
}
