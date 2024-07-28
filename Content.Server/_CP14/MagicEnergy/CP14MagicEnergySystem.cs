using Content.Server.Popups;
using Content.Shared._CP14.MagicEnergy;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Robust.Server.GameObjects;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.MagicEnergy;

public sealed partial class CP14MagicEnergySystem : SharedCP14MagicEnergySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly PointLightSystem _light = default!;
    [Dependency] private readonly CP14MagicEnergyCrystalSlotSystem _magicSlot = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        InitializeDraw();
        InitializeScanner();
        InitializeRelay();

        SubscribeLocalEvent<CP14MagicEnergyPointLightControllerComponent, CP14MagicEnergyLevelChangeEvent>(OnEnergyChange);
    }

    private void OnEnergyChange(Entity<CP14MagicEnergyPointLightControllerComponent> ent, ref CP14MagicEnergyLevelChangeEvent args)
    {
        if (!TryComp<PointLightComponent>(ent, out var light))
            return;

        var lightEnergy = MathHelper.Lerp(ent.Comp.MinEnergy, ent.Comp.MaxEnergy, (float)(args.NewValue / args.MaxValue));
        _light.SetEnergy(ent, lightEnergy, light);
    }

    private void OnMapInit(Entity<CP14MagicEnergyDrawComponent> ent, ref MapInitEvent args)
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

        args.PushMarkup(GetEnergyExaminedText(ent, magicContainer));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateDraw(frameTime);
        UpdateRelay(frameTime);
    }
}
