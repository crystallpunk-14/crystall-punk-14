using System.Text;
using Content.Shared._CP14.MagicEnergy;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Stacks;
using Content.Shared.Throwing;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._CP14.MagicEssence;

public partial class CP14MagicEssenceSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedCP14MagicEnergySystem _magicEnergy = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicEssenceContainerComponent, ExaminedEvent>(OnExamine);

        SubscribeLocalEvent<CP14MagicEssenceScannerComponent, CP14MagicEssenceScanEvent>(OnMagicScanAttempt);
        SubscribeLocalEvent<CP14MagicEssenceScannerComponent, InventoryRelayedEvent<CP14MagicEssenceScanEvent>>((e, c, ev) => OnMagicScanAttempt(e, c, ev.Args));

        SubscribeLocalEvent<CP14MagicEssenceSplitterComponent, CP14MagicEnergyOverloadEvent>(OnEnergyOverload);

        SubscribeLocalEvent<CP14MagicEssenceCollectorComponent, CP14SlotCrystalPowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<CP14MagicEssenceCollectorComponent, StartCollideEvent>(OnCollectorCollide);
    }

    private void OnPowerChanged(Entity<CP14MagicEssenceCollectorComponent> ent, ref CP14SlotCrystalPowerChangedEvent args)
    {
        if (args.Powered)
            EnsureComp<CP14MagicEssenceAttractorComponent>(ent);
        else
            RemCompDeferred<CP14MagicEssenceAttractorComponent>(ent);
    }

    private void OnCollectorCollide(Entity<CP14MagicEssenceCollectorComponent> ent, ref StartCollideEvent args)
    {
        if (TryComp<CP14MagicEnergyCrystalSlotComponent>(ent, out var energySlot) && !energySlot.Powered)
            return;

        if (!TryComp<CP14MagicEssenceComponent>(args.OtherEntity, out var essenceComp))
            return;

        if (!TryComp<SolutionContainerManagerComponent>(args.OtherEntity, out var essenceSolutionManager))
            return;

        if (!TryComp<SolutionContainerManagerComponent>(ent, out var collectorSolutionManager))
            return;

        if (!_solution.TryGetSolution((args.OtherEntity, essenceSolutionManager), essenceComp.Solution, out var essenceSoln, out var essenceSolution))
            return;

        if (!_solution.TryGetSolution((ent, collectorSolutionManager), ent.Comp.Solution, out var collectorSoln, out var collectorSolution))
            return;

        if (!_solution.TryTransferSolution(collectorSoln.Value, essenceSolution, essenceSolution.Volume))
            return;

        _audio.PlayPvs(essenceComp.ConsumeSound, ent);

        if (_net.IsServer)
            QueueDel(args.OtherEntity);
    }

    private void OnEnergyOverload(Entity<CP14MagicEssenceSplitterComponent> ent, ref CP14MagicEnergyOverloadEvent args)
    {
        if (!TryComp<CP14MagicEnergyContainerComponent>(ent, out var energyContainer))
            return;

        _magicEnergy.ChangeEnergy(ent, -energyContainer.Energy, out _, out _, energyContainer, safe: true);

        //TODO move to server
        if (_net.IsClient)
            return;

        var entities = _lookup.GetEntitiesInRange(ent, 0.5f, LookupFlags.Uncontained);
        foreach (var entUid in entities)
        {
            var splitting = !(ent.Comp.Whitelist is not null && !_whitelist.IsValid(ent.Comp.Whitelist, entUid));
            if (splitting)
                TrySplitToEssence(entUid);

            //Vector from splitter to item
            var dir = (Transform(entUid).Coordinates.Position - Transform(ent).Coordinates.Position).Normalized() * ent.Comp.ThrowForce;
            _throwing.TryThrow(entUid, dir, ent.Comp.ThrowForce);
        }
        SpawnAttachedTo(ent.Comp.ImpactEffect, Transform(ent).Coordinates);
    }

    private void OnMagicScanAttempt(EntityUid uid, CP14MagicEssenceScannerComponent component, CP14MagicEssenceScanEvent args)
    {
        args.CanScan = true;
    }

    private bool TrySplitToEssence(EntityUid uid)
    {
        if (!TryComp<CP14MagicEssenceContainerComponent>(uid, out var essenceContainer))
            return false;

   	    var count = 1;

        if (TryComp<StackComponent>(uid, out var stack))
        {
            count = stack.Count;
        }

        foreach (var essence in essenceContainer.Essences)
        {
            if (_proto.TryIndex(essence.Key, out var magicType))
            {
                for (var i = 0; i < essence.Value; i++)
                {
                    for (var j = 0; j < count; j++)
                    {
                        var spawned = SpawnAtPosition(magicType.EssenceProto, Transform(uid).Coordinates);
                        _transform.AttachToGridOrMap(spawned);
                    }
                }
            }
        }

        QueueDel(uid);
        return true;
    }

    private void OnExamine(Entity<CP14MagicEssenceContainerComponent> ent, ref ExaminedEvent args)
    {
        var scanEvent = new CP14MagicEssenceScanEvent();
        RaiseLocalEvent(args.Examiner, scanEvent);

        if (!scanEvent.CanScan)
            return;

        var count = 1;

        if (TryComp<StackComponent>(ent, out var stack))
        {
            count = stack.Count;
        }


        var sb = new StringBuilder();
        sb.Append(Loc.GetString("cp14-magic-essence-title") + "\n");
        foreach (var essence in ent.Comp.Essences)
        {
            if (_proto.TryIndex(essence.Key, out var magicType))
            {
                sb.Append($"[color={magicType.Color.ToHex()}]{Loc.GetString(magicType.Name)}[/color]: x{essence.Value * count}\n");
            }
        }

        args.PushMarkup(sb.ToString());
    }
}

public sealed class CP14MagicEssenceScanEvent : EntityEventArgs, IInventoryRelayEvent
{
    public bool CanScan;
    public SlotFlags TargetSlots { get; } = SlotFlags.EYES;
}
