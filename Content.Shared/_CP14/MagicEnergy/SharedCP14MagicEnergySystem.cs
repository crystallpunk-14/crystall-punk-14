using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Inventory;

namespace Content.Shared._CP14.MagicEnergy;

public partial class SharedCP14MagicEnergySystem : EntitySystem
{
    public string GetEnergyExaminedText(EntityUid uid, CP14MagicEnergyContainerComponent ent)
    {
        var power = (int)((ent.Energy / ent.MaxEnergy) * 100);

        var color = "#3fc488";
        if (power < 66)
            color = "#f2a93a";
        if (power < 33)
            color = "#c23030";

        return Loc.GetString("cp14-magic-energy-scan-result",
            ("item", MetaData(uid).EntityName),
            ("power", power),
            ("color", color));
    }
}

/// <summary>
/// It's triggered when the energy change in MagicEnergyContainer
/// </summary>
public sealed class CP14MagicEnergyLevelChangeEvent : EntityEventArgs
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
