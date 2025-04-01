using Content.Server._CP14.GameTicking.Rules;
using Content.Shared._CP14.WeatherControl;
using Content.Shared.Destructible.Thresholds;

namespace Content.Server._CP14.WeatherControl;

/// <summary>
/// is the controller that hangs on the prototype map. It regulates which weather rules are run and where they are run.
/// </summary>
[RegisterComponent, AutoGenerateComponentPause, Access(typeof(CP14WeatherControllerSystem), typeof(CP14WeatherRule))]
public sealed partial class CP14WeatherControllerComponent : Component
{
    [DataField]
    public bool Enabled = true;

    /// <summary>
    /// random time with no weather.
    /// </summary>
    [DataField]
    public MinMax ClearDuration = new(120,1200);

    [DataField]
    public HashSet<CP14WeatherData> Entries = new();

    [DataField, AutoPausedField]
    public TimeSpan NextWeatherTime = TimeSpan.Zero;
}
