using Content.Server._CP14.MagicEnergy.Components;
using Content.Shared._CP14.MagicEnergy;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Inventory;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Server._CP14.MagicEnergy;

public sealed partial class CP14MagicEnergySystem : SharedCP14MagicEnergySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly PointLightSystem _light = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicEnergySimpleRegenerationComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CP14MagicEnergyPointLightControllerComponent, CP14MagicEnergyChangeEvent>(OnEnergyChange);

        SubscribeLocalEvent<CP14MagicEnergyExaminableComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<CP14MagicEnergyScannerComponent, CP14MagicEnergyScanEvent>(OnMagicScanAttempt);
        SubscribeLocalEvent<CP14MagicEnergyScannerComponent, InventoryRelayedEvent<CP14MagicEnergyScanEvent>>((e, c, ev) => OnMagicScanAttempt(e, c, ev.Args));
    }

    private void OnEnergyChange(Entity<CP14MagicEnergyPointLightControllerComponent> ent, ref CP14MagicEnergyChangeEvent args)
    {
        if (!TryComp<PointLightComponent>(ent, out var light))
            return;

        var lightEnergy = MathHelper.Lerp(ent.Comp.MinEnergy, ent.Comp.MaxEnergy, (float)(args.NewValue / args.MaxValue));
        _light.SetEnergy(ent, lightEnergy, light);
    }

    private void OnMapInit(Entity<CP14MagicEnergySimpleRegenerationComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextUpdateTime = _gameTiming.CurTime + TimeSpan.FromSeconds(ent.Comp.Delay);
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

            ChangeEnergy(uid, magicContainer, regenerator.Energy, safe: true);
        }
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
            RaiseLocalEvent(uid, new CP14MagicEnergyChangeEvent()
            {
                OldValue = component.Energy,
                NewValue = newEnergy,
                MaxValue = component.MaxEnergy,
            });

            if (component.Energy == 0)
            {
                RaiseLocalEvent(uid, new CP14MagicEnergyOutEvent()
                {
                });
            }
        }
    }
}
