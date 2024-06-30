using Robust.Shared.GameStates;

namespace Content.Shared._CP14.Farming;

/// <summary>
/// The backbone of any plant. Provides common variables for the plant to other components, and a link to the soil
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), Access(typeof(CP14SharedFarmingSystem))]
public sealed partial class CP14PlantComponent : Component
{
    /// <summary>
    /// Soil link. May be null, as not all plants in the world grow on entity soil (e.g. wild shrubs)
    /// </summary>
    public EntityUid? SoilUid;

    /// <summary>
    /// The ability to consume a resource for growing
    /// </summary>
    [DataField]
    public float Energy = 0f;

    [DataField]
    public float EnergyMax = 100f;

    /// <summary>
    /// resource consumed for growth
    /// </summary>
    [DataField]
    public float Resource = 0f;

    [DataField]
    public float ResourceMax = 100f;

    /// <summary>
    /// Plant growth status, 0 to 1
    /// </summary>
    [DataField, AutoNetworkedField]
    public float GrowthLevel = 0f;

    [DataField(serverOnly: true)]
    public float UpdateFrequency = 60f;

    [DataField(serverOnly: true)]
    public TimeSpan NextUpdateTime = TimeSpan.Zero;

    [DataField(serverOnly: true)]
    public TimeSpan Age = TimeSpan.Zero;
}

/// <summary>
/// Is called periodically at random intervals on the plant.
/// </summary>
public sealed class CP14PlantUpdateEvent : EntityEventArgs
{
    public readonly Entity<CP14PlantComponent> Plant;
    public float EnergyDelta = 0f;
    public float ResourceDelta = 0f;

    public CP14PlantUpdateEvent(Entity<CP14PlantComponent> comp)
    {
        Plant = comp;
    }
}

/// <summary>
/// is called after CP14PlantUpdateEvent when all value changes have already been calculated.
/// </summary>
public sealed class CP14AfterPlantUpdateEvent : EntityEventArgs
{
    public readonly Entity<CP14PlantComponent> Plant;
    public CP14AfterPlantUpdateEvent(Entity<CP14PlantComponent> comp)
    {
        Plant = comp;
    }
}
