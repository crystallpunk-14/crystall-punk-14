using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared._CP14.MagicEssence;
using Content.Shared.Alert;
using Content.Shared.Audio;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Rounding;

namespace Content.Shared._CP14.MagicEnergy;

public partial class SharedCP14MagicEnergySystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedAmbientSoundSystem _ambient = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CP14MagicEnergyAmbientSoundComponent, CP14SlotCrystalPowerChangedEvent>(OnSlotPowerChanged);

        SubscribeLocalEvent<CP14MagicEnergyContainerComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<CP14MagicEnergyContainerComponent, ComponentShutdown>(OnComponentShutdown);

        SubscribeLocalEvent<CP14MagicEnergyExaminableComponent, ExaminedEvent>(OnExamined);
    }

    private void OnSlotPowerChanged(Entity<CP14MagicEnergyAmbientSoundComponent> ent, ref CP14SlotCrystalPowerChangedEvent args)
    {
        if (TryComp<AmbientSoundComponent>(ent, out var ambient))
        {
            _ambient.SetAmbience(ent, args.Powered);
        }
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

    public void ChangeEnergy(EntityUid uid,
        FixedPoint2 energy,
        out FixedPoint2 changedEnergy,
        out FixedPoint2 overloadEnergy,
        CP14MagicEnergyContainerComponent? component = null,
        bool safe = false)
    {
        changedEnergy = 0;
        overloadEnergy = 0;

        if (!Resolve(uid, ref component, false))
            return;

        if (!safe)
        {
            //Overload
            if (component.Energy + energy > component.MaxEnergy && component.UnsafeSupport)
            {
                overloadEnergy = (component.Energy + energy) - component.MaxEnergy;
                    RaiseLocalEvent(uid,
                        new CP14MagicEnergyOverloadEvent()
                        {
                            OverloadEnergy = (component.Energy + energy) - component.MaxEnergy,
                        });
            }

            //Burn out
            if (component.Energy + energy < 0 && component.UnsafeSupport)
            {
                overloadEnergy = component.Energy + energy;
                    RaiseLocalEvent(uid,
                        new CP14MagicEnergyBurnOutEvent()
                        {
                            BurnOutEnergy = -energy - component.Energy
                        });
            }
        }

        var oldEnergy = component.Energy;
        var newEnergy = Math.Clamp((float)component.Energy + (float)energy, 0, (float)component.MaxEnergy);

        changedEnergy = newEnergy - oldEnergy;
        component.Energy = newEnergy;

        if (oldEnergy != newEnergy)
        {
            RaiseLocalEvent(uid,
                new CP14MagicEnergyLevelChangeEvent()
                {
                    OldValue = oldEnergy,
                    NewValue = newEnergy,
                    MaxValue = component.MaxEnergy,
                });
        }

        UpdateMagicAlert((uid, component));
    }

    public void TransferEnergy(EntityUid from,
        EntityUid to,
        FixedPoint2 energy,
        out FixedPoint2 changedEnergy,
        out FixedPoint2 overloadEnergy,
        CP14MagicEnergyContainerComponent? fromComponent = null,
        CP14MagicEnergyContainerComponent? toComponent = null,
        bool safe = false)
    {
        changedEnergy = 0;
        overloadEnergy = 0;

        if (!Resolve(from, ref fromComponent) || !Resolve(to, ref toComponent))
            return;

        var transferEnergy = energy;
        //We check how much space is left in the container so as not to overload it, but only if it does not support overloading
        if (!toComponent.UnsafeSupport || safe)
        {
            var freeSpace = toComponent.MaxEnergy - toComponent.Energy;
            transferEnergy = FixedPoint2.Min(freeSpace, energy);
        }

        ChangeEnergy(from, -transferEnergy, out var change, out var overload, fromComponent, safe);
        ChangeEnergy(to , -(change + overload), out changedEnergy, out overloadEnergy, toComponent, safe);
    }

    public bool HasEnergy(EntityUid uid, FixedPoint2 energy, CP14MagicEnergyContainerComponent? component = null, bool safe = false)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (safe == false && component.UnsafeSupport)
            return true;

        return component.Energy >= energy;
    }

    private void UpdateMagicAlert(Entity<CP14MagicEnergyContainerComponent> ent)
    {
        if (ent.Comp.MagicAlert == null)
            return;

        var level = ContentHelpers.RoundToLevels(MathF.Max(0f, (float)ent.Comp.Energy),
            (float)ent.Comp.MaxEnergy,
            _alerts.GetMaxSeverity(ent.Comp.MagicAlert.Value));
        _alerts.ShowAlert(ent, ent.Comp.MagicAlert.Value, (short)level);
    }

    private void OnExamined(Entity<CP14MagicEnergyExaminableComponent> ent, ref ExaminedEvent args)
    {
        if (!TryComp<CP14MagicEnergyContainerComponent>(ent, out var magicContainer))
            return;

        if (!args.IsInDetailsRange)
            return;

        args.PushMarkup(GetEnergyExaminedText(ent, magicContainer));
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
