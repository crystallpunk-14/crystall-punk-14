using Content.Shared._CP14.WeatherControl;
using Content.Shared.Destructible.Thresholds;

namespace Content.Server._CP14.WeatherControl;

/// <summary>
/// is the controller that hangs on the prototype map. It regulates which weather rules are run and where they are run.
/// </summary>
[RegisterComponent, AutoGenerateComponentPause, Access(typeof(CP14WeatherControllerSystem))]
public sealed partial class CP14WeatherControllerComponent : Component
{
    /// <summary>
    /// random time with no weather.
    /// </summary>
    [DataField]
    public MinMax ClearDuration = new(60,600);

    [DataField]
    public HashSet<CP14WeatherData> Entries = new();

    [DataField, AutoPausedField]
    public TimeSpan NextWeatherTime = TimeSpan.Zero;
}
