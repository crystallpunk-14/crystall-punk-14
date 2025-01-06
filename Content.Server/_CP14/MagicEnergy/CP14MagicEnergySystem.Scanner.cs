using System.Numerics;
using Content.Server._CP14.MagicEnergy.Components;
using Content.Shared._CP14.MagicEnergy;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;

namespace Content.Server._CP14.MagicEnergy;

public partial class CP14MagicEnergySystem
{

    private void InitializeScanner()
    {
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

        if (!args.IsInDetailsRange)
            return;

        //var scanEvent = new CP14MagicEnergyScanEvent();
        //RaiseLocalEvent(args.Examiner, scanEvent);
//
        //if (!scanEvent.CanScan)
        //    return;

        args.PushMarkup(GetEnergyExaminedText(ent, magicContainer));
    }
}
