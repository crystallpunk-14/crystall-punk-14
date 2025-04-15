namespace Content.Shared._CP14.WeatherEffect;

/// <summary>
/// Allows weather to fill said solution with liquids if said entity is outdoors
/// </summary>
[RegisterComponent]
public sealed partial class CP14WeatherRefillableComponent : Component
{
    [DataField(required: true)]
    public string Solution = string.Empty;
}
