using Content.Server.Chemistry.EntitySystems;

namespace Content.Server._CP14.Temperature;

/// <summary>
/// Adds thermal energy from FlammableComponent to solutions placed on it.
/// </summary>
[RegisterComponent, Access(typeof(SolutionHeaterSystem))]
public sealed partial class CP14FlammableSolutionHeaterComponent : Component
{
}
