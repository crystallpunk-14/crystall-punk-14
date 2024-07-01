namespace Content.Server._CP14.Farming.Components;

/// <summary>
/// when it init, it automatically attaches itself by its roots to the soil beneath it.
/// </summary>
[RegisterComponent, Access(typeof(CP14FarmingSystem))]
public sealed partial class CP14PlantAutoRootComponent : Component
{
}
