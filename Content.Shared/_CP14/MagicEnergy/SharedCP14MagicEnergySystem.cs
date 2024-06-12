namespace Content.Shared._CP14.MagicEnergy;

public partial class SharedCP14MagicEnergySystem : EntitySystem
{

}

/// <summary>
/// It's triggered when the power runs out in MagicEnergyContainer
/// </summary>
public sealed class CP14MagicEnergyOutEvent : EntityEventArgs
{
    public EntityUid MagicContainer;
}

/// <summary>
/// It's triggered when more energy enters the MagicEnergyContainer than it can hold.
/// </summary>
public sealed class CP14MagicEnergyOverloadEvent : EntityEventArgs
{
    public EntityUid MagicContainer;
    public float OverloadEnergy;
}

/// <summary>
/// It's triggered they something try to get energy out of MagicEnergyContainer that is lacking there.
/// </summary>
public sealed class CP14MagicEnergyBurnOutEvent : EntityEventArgs
{
    public EntityUid MagicContainer;
    public float BurnOutEnergy;
}
