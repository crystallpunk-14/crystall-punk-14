using JetBrains.Annotations;

namespace Content.Shared._CP14.WeatherEffect;


[DataDefinition]
public sealed partial record CP14WeatherEffectConfig
{
    [DataField]
    public float Frequency = 10f;

    [DataField]
    public List<CP14WeatherEffect> Effects = new();

    /// <summary>
    /// Determines which entities can be affected by weather. This is done for optimization, as it is very hard to make an EntityWhitelist for each entity on the map.
    /// </summary>
    [DataField]
    public WeatherEntityFilter Filter = WeatherEntityFilter.All;
}

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14WeatherEffect
{
    public abstract void ApplyEffect(IEntityManager entManager, EntityUid target);
}


public enum WeatherEntityFilter : byte
{
    All,
    Flammable,
}
