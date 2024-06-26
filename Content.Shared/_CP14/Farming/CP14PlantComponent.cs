using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Farming;

/// <summary>
/// The backbone of any plant. Provides common variables for the plant to other components, and a link to the soil
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class CP14PlantComponent : Component
{
    private float _resource;
    private float _health;
    private float _energy;
    private float _growth;

    /// <summary>
    /// Soil link. May be null, as not all plants in the world grow on entity soil (e.g. wild shrubs)
    /// </summary>
    public EntityUid? SoilUid;

    /// <summary>
    /// The ability to consume a resource for growing
    /// </summary>
    [DataField]
    public float Energy
    {
        set => _energy = MathHelper.Clamp(value, 0, MaxEnergy);
        get => _energy;
    }

    [DataField]
    public float MaxEnergy = 100f;


    /// <summary>
    /// resource consumed for growth
    /// </summary>
    [DataField]
    public float Resource
    {
        set => _resource = MathHelper.Clamp(value, 0, MaxResource);
        get => _resource;
    }

    [DataField]
    public float MaxResource = 100f;


    [DataField]
    public float Health
    {
        set => _health = MathHelper.Clamp(value, 0, MaxHealth);
        get => _health;
    }

    /// <summary>
    ///
    /// </summary>
    [DataField]
    public float MaxHealth = 10f;

    /// <summary>
    /// Plant growth status, 0 to 1
    /// </summary>
    [DataField, AutoNetworkedField]
    public float GrowthLevel
    {
        set => _growth = MathHelper.Clamp01(value);
        get => _growth;
    }

    [DataField(serverOnly: true)]
    public TimeSpan UpdateFrequency = TimeSpan.FromSeconds(90f);

    [DataField(serverOnly: true)]
    public TimeSpan NextUpdateTime = TimeSpan.Zero;
}

/// <summary>
/// Is called periodically at random intervals on the plant.
/// </summary>
public sealed class CP14AfterPlantUpdateEvent : EntityEventArgs
{
    public readonly Entity<CP14PlantComponent> Plant;

    public CP14AfterPlantUpdateEvent(Entity<CP14PlantComponent> comp)
    {
        Plant = comp;
    }
}

public sealed class CP14PlantEnergyUpdateEvent : CancellableEntityEventArgs
{
    public float Energy = 0f;
}

public sealed class CP14PlantResourceUpdateEvent : CancellableEntityEventArgs
{
    public float Resource = 0f;
}
