using Robust.Shared.GameStates;

namespace Content.Shared._CP14.Farming.Components;

/// <summary>
/// The backbone of any plant. Provides common variables for the plant to other components
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause, AutoGenerateComponentState(true), Access(typeof(CP14SharedFarmingSystem))]
public sealed partial class CP14PlantComponent : Component
{
    /// <summary>
    /// The ability to consume a resource for growing
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Energy = 30f;

    [DataField, AutoNetworkedField]
    public float EnergyMax = 100f;

    /// <summary>
    /// resource consumed for growth
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Resource = 30f;

    [DataField, AutoNetworkedField]
    public float ResourceMax = 100f;

    /// <summary>
    /// Plant growth status, from 0 to 1
    /// </summary>
    [DataField, AutoNetworkedField]
    public float GrowthLevel = 0f;

    [DataField]
    public float UpdateFrequency = 60f;

    [DataField, AutoPausedField]
    public TimeSpan NextUpdateTime = TimeSpan.Zero;

    [DataField(required: true)]
    public string Solution = string.Empty;
}

/// <summary>
/// Is called periodically at random intervals on the plant.
/// </summary>
public sealed class CP14PlantUpdateEvent(Entity<CP14PlantComponent> comp) : EntityEventArgs
{
    public readonly Entity<CP14PlantComponent> Plant = comp;
    public float EnergyDelta = 0f;
    public float ResourceDelta = 0f;
}

/// <summary>
/// is called after CP14PlantUpdateEvent when all value changes have already been calculated.
/// </summary>
public sealed class CP14AfterPlantUpdateEvent(Entity<CP14PlantComponent> comp) : EntityEventArgs
{
    public readonly Entity<CP14PlantComponent> Plant = comp;
}
