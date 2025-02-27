using System.Text;
using Content.Shared._CP14.MagicEnergy;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Throwing;
using Content.Shared.Whitelist;
using Robust.Shared.Network;
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

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicEssenceContainerComponent, ExaminedEvent>(OnExamine);

        SubscribeLocalEvent<CP14MagicEssenceScannerComponent, CP14MagicEssenceScanEvent>(OnMagicScanAttempt);
        SubscribeLocalEvent<CP14MagicEssenceScannerComponent, InventoryRelayedEvent<CP14MagicEssenceScanEvent>>((e, c, ev) => OnMagicScanAttempt(e, c, ev.Args));

        SubscribeLocalEvent<CP14MagicEssenceSplitterComponent, CP14MagicEnergyOverloadEvent>(OnEnergyOverload);
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

        foreach (var essence in essenceContainer.Essences)
        {
            if (_proto.TryIndex(essence.Key, out var magicType))
            {
                for (var i = 0; i < essence.Value; i++)
                {
                    var spawned = SpawnAtPosition(magicType.EssenceProto, Transform(uid).Coordinates);
                    _transform.AttachToGridOrMap(spawned);
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

        var sb = new StringBuilder();
        sb.Append(Loc.GetString("cp14-magic-essence-title") + "\n");
        foreach (var essence in ent.Comp.Essences)
        {
            if (_proto.TryIndex(essence.Key, out var magicType))
            {
                sb.Append($"[color={magicType.Color.ToHex()}]{Loc.GetString(magicType.Name)}[/color]: x{essence.Value}\n");
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
