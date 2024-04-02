using Content.Server.Temperature.Systems;

namespace Content.Server.CrystallPunk.FlammableEntityHeater;

/// <summary>
/// Adds thermal energy from FlammableComponent to entities with <see cref="TemperatureComponent"/> placed on it.
/// </summary>
[RegisterComponent, Access(typeof(EntityHeaterSystem))]
public sealed partial class CPFlammableEntityHeaterComponent : Component
{
}
