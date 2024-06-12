using Content.Server._CP14.MagicEnergy.Components;
using Content.Shared._CP14.MagicEnergy;
using Robust.Shared.Timing;

namespace Content.Server._CP14.MagicEnergy;

public sealed partial class CP14MagicEnergySystem : SharedCP14MagicEnergySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        base.Initialize();


    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14MagicEnergyRegeneratorComponent, CP14MagicEnergyContainerComponent>();
        while (query.MoveNext(out var uid, out var regenerator, out var magicContainer))
        {
            if (regenerator.NextUpdateTime >= _gameTiming.CurTime)
                continue;

            regenerator.NextUpdateTime = _gameTiming.CurTime + TimeSpan.FromSeconds(regenerator.Delay);

            ChangeEnergy(uid, magicContainer, regenerator.Energy);
        }
    }

    public bool TryConsumeEnergy(EntityUid uid, float energy, CP14MagicEnergyContainerComponent? component = null)
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

        ChangeEnergy(uid, component, -energy);
        return true;
    }

    public void ChangeEnergy(EntityUid uid, CP14MagicEnergyContainerComponent component, float energy)
    {
        //Overload
        if (component.Energy + energy > component.MaxEnergy)
        {
            RaiseLocalEvent(new CP14MagicEnergyOverloadEvent()
            {
                MagicContainer = uid,
                OverloadEnergy = (component.Energy + energy) - component.MaxEnergy,
            });
        }

        //Burn out
        if (component.Energy + energy < 0)
        {
            RaiseLocalEvent(new CP14MagicEnergyBurnOutEvent()
            {
                MagicContainer = uid,
                BurnOutEnergy = -energy - component.Energy
            });
        }

        component.Energy = Math.Clamp(component.Energy + energy, 0, component.MaxEnergy);

        if (component.Energy == 0)
        {
            RaiseLocalEvent(new CP14MagicEnergyOutEvent()
            {
                MagicContainer = uid,
            });
        }
    }
}
