using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared._CP14.MagicEssence;
using Content.Shared.Alert;
using Content.Shared.Audio;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Rejuvenate;
using Content.Shared.Rounding;

namespace Content.Shared._CP14.MagicEnergy;

public abstract class SharedCP14MagicEnergySystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedAmbientSoundSystem _ambient = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CP14MagicEnergyContainerComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<CP14MagicEnergyContainerComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeLocalEvent<CP14MagicEnergyContainerComponent, RejuvenateEvent>(OnRejuvenate);

        SubscribeLocalEvent<CP14MagicEnergyExaminableComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<CP14MagicEnergyAmbientSoundComponent, CP14SlotCrystalPowerChangedEvent>(OnSlotPowerChanged);
    }

    private void OnRejuvenate(Entity<CP14MagicEnergyContainerComponent> ent, ref RejuvenateEvent args)
    {
        ChangeEnergy((ent, ent.Comp), ent.Comp.MaxEnergy - ent.Comp.Energy, out var deltaEnergy, out var overloadEnergy, true);
    }

    private void OnComponentStartup(Entity<CP14MagicEnergyContainerComponent> ent, ref ComponentStartup args)
    {
        UpdateMagicAlert(ent);
    }

    private void OnComponentShutdown(Entity<CP14MagicEnergyContainerComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.MagicAlert is null)
            return;

        _alerts.ClearAlert(ent, ent.Comp.MagicAlert.Value);
    }

    private void OnExamined(Entity<CP14MagicEnergyExaminableComponent> ent, ref ExaminedEvent args)
    {
        if (!TryComp<CP14MagicEnergyContainerComponent>(ent, out var magicContainer))
            return;

        if (!args.IsInDetailsRange)
            return;

        args.PushMarkup(GetEnergyExaminedText((ent, magicContainer)));
    }

    private void OnSlotPowerChanged(Entity<CP14MagicEnergyAmbientSoundComponent> ent, ref CP14SlotCrystalPowerChangedEvent args)
    {
        _ambient.SetAmbience(ent, args.Powered);
    }

    private void UpdateMagicAlert(Entity<CP14MagicEnergyContainerComponent> ent)
    {
        if (ent.Comp.MagicAlert is null)
            return;

        var level = ContentHelpers.RoundToLevels(
            MathF.Max(0f, (float) ent.Comp.Energy),
            (float) ent.Comp.MaxEnergy,
            _alerts.GetMaxSeverity(ent.Comp.MagicAlert.Value));

        _alerts.ShowAlert(ent, ent.Comp.MagicAlert.Value, (short) level);
    }

    public void ChangeEnergy(Entity<CP14MagicEnergyContainerComponent?> ent,
        FixedPoint2 energy,
        out FixedPoint2 deltaEnergy,
        out FixedPoint2 overloadEnergy,
        bool safe = false)
    {
        deltaEnergy = 0;
        overloadEnergy = 0;

        if (!Resolve(ent, ref ent.Comp, false))
            return;

        if (!safe)
        {
            // Overload
            if (ent.Comp.Energy + energy > ent.Comp.MaxEnergy && ent.Comp.UnsafeSupport)
            {
                overloadEnergy = ent.Comp.Energy + energy - ent.Comp.MaxEnergy;
                RaiseLocalEvent(ent, new CP14MagicEnergyOverloadEvent(overloadEnergy));
            }

            // Burn out
            if (ent.Comp.Energy + energy < 0 && ent.Comp.UnsafeSupport)
            {
                overloadEnergy = ent.Comp.Energy + energy;
                RaiseLocalEvent(ent, new CP14MagicEnergyBurnOutEvent(-energy - ent.Comp.Energy));
            }
        }

        var oldEnergy = ent.Comp.Energy;
        var newEnergy = Math.Clamp((float) ent.Comp.Energy + (float) energy, 0, (float) ent.Comp.MaxEnergy);

        deltaEnergy = newEnergy - oldEnergy;
        ent.Comp.Energy = newEnergy;

        if (oldEnergy != newEnergy)
            RaiseLocalEvent(ent, new CP14MagicEnergyLevelChangeEvent(oldEnergy, newEnergy, ent.Comp.MaxEnergy));

        UpdateMagicAlert((ent, ent.Comp));
    }

    public void TransferEnergy(Entity<CP14MagicEnergyContainerComponent?> sender,
        Entity<CP14MagicEnergyContainerComponent?> receiver,
        FixedPoint2 energy,
        out FixedPoint2 deltaEnergy,
        out FixedPoint2 overloadEnergy,
        bool safe = false)
    {
        deltaEnergy = 0;
        overloadEnergy = 0;

        if (!Resolve(sender, ref sender.Comp) || !Resolve(receiver, ref receiver.Comp))
            return;

        var transferEnergy = energy;
        //We check how much space is left in the container so as not to overload it, but only if it does not support overloading
        if (!receiver.Comp.UnsafeSupport || safe)
        {
            var freeSpace = receiver.Comp.MaxEnergy - receiver.Comp.Energy;
            transferEnergy = FixedPoint2.Min(freeSpace, energy);
        }

        ChangeEnergy(sender, -transferEnergy, out var change, out var overload, safe);
        ChangeEnergy(receiver , -(change + overload), out deltaEnergy, out overloadEnergy, safe);
    }

    public bool HasEnergy(EntityUid uid, FixedPoint2 energy, CP14MagicEnergyContainerComponent? component = null, bool safe = false)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (!safe && component.UnsafeSupport)
            return true;

        return component.Energy >= energy;
    }

    public string GetEnergyExaminedText(Entity<CP14MagicEnergyContainerComponent> ent)
    {
        var power = (int) (ent.Comp.Energy / ent.Comp.MaxEnergy * 100);

        // TODO: customization for examined

        var color = "#3fc488";
        if (power < 66)
            color = "#f2a93a";

        if (power < 33)
            color = "#c23030";

        return Loc.GetString("cp14-magic-energy-scan-result",
            ("item", MetaData(ent).EntityName),
            ("power", power),
            ("color", color));
    }

    public void ChangeMaximumEnergy(Entity<CP14MagicEnergyContainerComponent?> ent, FixedPoint2 energy)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        ent.Comp.MaxEnergy += energy;

        ChangeEnergy(ent, energy, out _, out _);
    }
}

/// <summary>
/// It's triggered when the energy change in MagicEnergyContainer
/// </summary>
public sealed class CP14MagicEnergyLevelChangeEvent : EntityEventArgs
{
    public readonly FixedPoint2 OldValue;
    public readonly FixedPoint2 NewValue;
    public readonly FixedPoint2 MaxValue;

    public CP14MagicEnergyLevelChangeEvent(FixedPoint2 oldValue, FixedPoint2 newValue, FixedPoint2 maxValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
        MaxValue = maxValue;
    }
}

/// <summary>
/// It's triggered when more energy enters the MagicEnergyContainer than it can hold.
/// </summary>
public sealed class CP14MagicEnergyOverloadEvent : EntityEventArgs
{
    public readonly FixedPoint2 OverloadEnergy;

    public CP14MagicEnergyOverloadEvent(FixedPoint2 overloadEnergy)
    {
        OverloadEnergy = overloadEnergy;
    }
}

/// <summary>
/// It's triggered they something try to get energy out of MagicEnergyContainer that is lacking there.
/// </summary>
public sealed class CP14MagicEnergyBurnOutEvent : EntityEventArgs
{
    public readonly FixedPoint2 BurnOutEnergy;

    public CP14MagicEnergyBurnOutEvent(FixedPoint2 burnOutEnergy)
    {
        BurnOutEnergy = burnOutEnergy;
    }
}
