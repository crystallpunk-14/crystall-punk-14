using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Inventory;

namespace Content.Shared._CP14.MagicEnergy;

public partial class SharedCP14MagicEnergySystem : EntitySystem
{
}

/// <summary>
/// It's triggered when the power runs out in MagicEnergyContainer
/// </summary>
public sealed class CP14MagicEnergyOutEvent : EntityEventArgs
{
}

/// <summary>
/// It's triggered when the energy change in MagicEnergyContainer
/// </summary>
public sealed class CP14MagicEnergyChangeEvent : EntityEventArgs
{
    public FixedPoint2 OldValue;
    public FixedPoint2 NewValue;
    public FixedPoint2 MaxValue;
}

/// <summary>
/// It's triggered when more energy enters the MagicEnergyContainer than it can hold.
/// </summary>
public sealed class CP14MagicEnergyOverloadEvent : EntityEventArgs
{
    public FixedPoint2 OverloadEnergy;
}

/// <summary>
/// It's triggered they something try to get energy out of MagicEnergyContainer that is lacking there.
/// </summary>
public sealed class CP14MagicEnergyBurnOutEvent : EntityEventArgs
{
    public FixedPoint2 BurnOutEnergy;
}

public sealed class CP14MagicEnergyScanEvent : EntityEventArgs, IInventoryRelayEvent
{
    public bool CanScan;
    public SlotFlags TargetSlots { get; } = SlotFlags.EYES;
}
