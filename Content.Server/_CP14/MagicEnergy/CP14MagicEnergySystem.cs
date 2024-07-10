using Content.Shared._CP14.MagicEnergy;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.FixedPoint;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Server._CP14.MagicEnergy;

public sealed partial class CP14MagicEnergySystem : SharedCP14MagicEnergySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly PointLightSystem _light = default!;
    [Dependency] private readonly CP14MagicEnergyCrystalSlotSystem _magicSlot = default!;

    public override void Initialize()
    {
        InitializeDraw();
        InitializeScanner();

        SubscribeLocalEvent<CP14MagicEnergyPointLightControllerComponent, CP14MagicEnergyLevelChangeEvent>(OnEnergyChange);
    }

    private void OnEnergyChange(Entity<CP14MagicEnergyPointLightControllerComponent> ent, ref CP14MagicEnergyLevelChangeEvent args)
    {
        if (!TryComp<PointLightComponent>(ent, out var light))
            return;

        var lightEnergy = MathHelper.Lerp(ent.Comp.MinEnergy, ent.Comp.MaxEnergy, (float)(args.NewValue / args.MaxValue));
        _light.SetEnergy(ent, lightEnergy, light);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateDraw(frameTime);
    }

    public bool TryConsumeEnergy(EntityUid uid, FixedPoint2 energy, CP14MagicEnergyContainerComponent? component = null, bool safe = false)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (energy <= 0)
            return true;

        // Attempting to absorb more energy than is contained in the carrier will still waste all the energy
        if (component.Energy < energy)
        {
            ChangeEnergy(uid, component, -component.Energy);
            return false;
        }

        ChangeEnergy(uid, component, -energy, safe);
        return true;
    }

    public void ChangeEnergy(EntityUid uid, CP14MagicEnergyContainerComponent component, FixedPoint2 energy, bool safe = false)
    {
        if (!safe)
        {
            //Overload
            if (component.Energy + energy > component.MaxEnergy)
            {
                RaiseLocalEvent(uid, new CP14MagicEnergyOverloadEvent
                {
                    OverloadEnergy = (component.Energy + energy) - component.MaxEnergy,
                });
            }

            //Burn out
            if (component.Energy + energy < 0)
            {
                RaiseLocalEvent(uid, new CP14MagicEnergyBurnOutEvent
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
            RaiseLocalEvent(uid, new CP14MagicEnergyLevelChangeEvent
            {
                OldValue = component.Energy,
                NewValue = newEnergy,
                MaxValue = component.MaxEnergy,
            });
        }
    }
}
