using Content.Server._CP14.MagicEnergy.Components;
using Content.Shared._CP14.MagicEnergy;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Robust.Shared.Timing;

namespace Content.Server._CP14.MagicEnergy;

public sealed partial class CP14MagicEnergySystem : SharedCP14MagicEnergySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicEnergyExaminableComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<CP14MagicEnergyScannerComponent, CP14MagicEnergyScanEvent>(OnMagicScanAttempt);
        SubscribeLocalEvent<CP14MagicEnergyScannerComponent, InventoryRelayedEvent<CP14MagicEnergyScanEvent>>((e, c, ev) => OnMagicScanAttempt(e, c, ev.Args));
    }

    private void OnMagicScanAttempt(EntityUid uid, CP14MagicEnergyScannerComponent component, CP14MagicEnergyScanEvent args)
    {
        args.CanScan = true;
    }

    private void OnExamined(Entity<CP14MagicEnergyExaminableComponent> ent, ref ExaminedEvent args)
    {
        if (!TryComp<CP14MagicEnergyContainerComponent>(ent, out var magicContainer))
            return;

        var scanEvent = new CP14MagicEnergyScanEvent();
        RaiseLocalEvent(args.Examiner, scanEvent);

        if (!scanEvent.CanScan)
            return;

        var power = (int)((magicContainer.Energy / magicContainer.MaxEnergy) * 100);

        var color = "#3fc488";
        if (power < 66)
            color = "#f2a93a";
        if (power < 33)
            color = "#c23030";

        args.PushMarkup(Loc.GetString("cp14-magic-energy-scan-result", ("item", MetaData(ent).EntityName), ("power", power), ("color", color)));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14MagicEnergySimpleRegenerationComponent, CP14MagicEnergyContainerComponent>();
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
