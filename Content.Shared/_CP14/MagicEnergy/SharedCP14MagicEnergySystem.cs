using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.Alert;
using Content.Shared.FixedPoint;
using Content.Shared.Inventory;
using Content.Shared.Rounding;

namespace Content.Shared._CP14.MagicEnergy;

public partial class SharedCP14MagicEnergySystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CP14MagicEnergyContainerComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<CP14MagicEnergyContainerComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnComponentStartup(Entity<CP14MagicEnergyContainerComponent> ent, ref ComponentStartup args)
    {
        UpdateMagicAlert(ent);
    }

    private void OnComponentShutdown(Entity<CP14MagicEnergyContainerComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.MagicAlert == null)
            return;

        _alerts.ClearAlert(ent, ent.Comp.MagicAlert.Value);
    }

    public string GetEnergyExaminedText(EntityUid uid, CP14MagicEnergyContainerComponent ent)
    {
        var power = (int)(ent.Energy / ent.MaxEnergy * 100);

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

    public void ChangeEnergy(EntityUid uid, FixedPoint2 energy, bool safe = false)
    {
        if (!TryComp<CP14MagicEnergyContainerComponent>(uid, out var energyContainer))
            return;

        ChangeEnergy(uid, energyContainer, energy, safe);
    }

    public void ChangeEnergy(EntityUid uid, CP14MagicEnergyContainerComponent component, FixedPoint2 energy, bool safe = false)
    {
        if (!safe)
        {
            //Overload
            if (component.Energy + energy > component.MaxEnergy)
            {
                RaiseLocalEvent(uid, new CP14MagicEnergyOverloadEvent()
                {
                    OverloadEnergy = (component.Energy + energy) - component.MaxEnergy,
                });
            }

            //Burn out
            if (component.Energy + energy < 0)
            {
                RaiseLocalEvent(uid, new CP14MagicEnergyBurnOutEvent()
                {
                    BurnOutEnergy = -energy - component.Energy
                });
            }
        }

        var oldEnergy = component.Energy;
        var newEnergy = Math.Clamp((float)component.Energy + (float)energy, 0, (float)component.MaxEnergy);
        component.Energy = newEnergy;

        if (oldEnergy != newEnergy)
        {
            RaiseLocalEvent(uid, new CP14MagicEnergyLevelChangeEvent()
            {
                OldValue = component.Energy,
                NewValue = newEnergy,
                MaxValue = component.MaxEnergy,
            });
        }

        UpdateMagicAlert((uid, component));
    }

    public bool HasEnergy(EntityUid uid, FixedPoint2 energy, CP14MagicEnergyContainerComponent? component = null, bool safe = false)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (safe == false)
            return true;

        return component.Energy > energy;
    }

    public bool TryConsumeEnergy(EntityUid uid, FixedPoint2 energy, CP14MagicEnergyContainerComponent? component = null, bool safe = false)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (energy <= 0)
            return true;

        // Attempting to absorb more energy than is contained in the container available only in non-safe methods (with container destruction)
        if (component.Energy < energy)
        {
            if (safe)
            {
                return false;
            }
            else
            {
                ChangeEnergy(uid, component, -energy, safe);
                return true;
            }
        }

        ChangeEnergy(uid, component, -energy, safe);
        return true;
    }

    private void UpdateMagicAlert(Entity<CP14MagicEnergyContainerComponent> ent)
    {
        if (ent.Comp.MagicAlert == null)
            return;

        var level = ContentHelpers.RoundToLevels(MathF.Max(0f, (float) ent.Comp.Energy), (float) ent.Comp.MaxEnergy, 10);
        _alerts.ShowAlert(ent, ent.Comp.MagicAlert.Value, (short)level);
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
