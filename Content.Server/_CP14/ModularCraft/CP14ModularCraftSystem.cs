using Content.Server.Item;
using Content.Shared._CP14.ModularCraft;
using Content.Shared._CP14.ModularCraft.Components;
using Content.Shared._CP14.ModularCraft.Prototypes;
using Content.Shared.Throwing;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.ModularCraft;

public sealed class CP14ModularCraftSystem : CP14SharedModularCraftSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ItemSystem _item = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14ModularCraftStartPointComponent, MapInitEvent>(OnStartPointMapInit);
        SubscribeLocalEvent<CP14ModularCraftStartPointComponent, CP14ModularCraftAddPartDoAfter>(OnAddedPart);
    }

    private void OnAddedPart(Entity<CP14ModularCraftStartPointComponent> ent, ref CP14ModularCraftAddPartDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!TryComp<CP14ModularCraftPartComponent>(args.Used, out var partComp))
            return;

        AddPartToFirstSlot(ent, (args.Used.Value, partComp));
        QueueDel(args.Used);

        //TODO: Sound

        args.Handled = true;
    }

    private void OnStartPointMapInit(Entity<CP14ModularCraftStartPointComponent> ent, ref MapInitEvent args)
    {
        foreach (var startSlot in ent.Comp.StartSlots)
        {
            ent.Comp.FreeSlots.Add(startSlot);
        }

        if (TryComp<CP14ModularCraftAutoAssembleComponent>(ent, out var autoAssemble))
        {
            foreach (var detail in autoAssemble.Details)
            {
                AddPartToFirstSlot(ent, detail);
            }
        }
    }
    private void AddPartToFirstSlot(Entity<CP14ModularCraftStartPointComponent> start,
        Entity<CP14ModularCraftPartComponent> part)
    {
        foreach (var partProto in part.Comp.PossibleParts)
        {
            if (!_proto.TryIndex(partProto, out var partIndexed))
                continue;

            if (!start.Comp.FreeSlots.Contains(partIndexed.TargetSlot))
                continue;

            if (TryAddPartToSlot(start, part, partProto, partIndexed.TargetSlot))
            {
                QueueDel(part);
            }
            return;
        }
    }
    private void AddPartToFirstSlot(Entity<CP14ModularCraftStartPointComponent> start,
        ProtoId<CP14ModularCraftPartPrototype> partProto)
    {
        if (!_proto.TryIndex(partProto, out var partIndexed))
            return;

        if (!start.Comp.FreeSlots.Contains(partIndexed.TargetSlot))
            return;

        TryAddPartToSlot(start, null, partProto, partIndexed.TargetSlot);
    }
    private bool TryAddPartToSlot(Entity<CP14ModularCraftStartPointComponent> start,
        Entity<CP14ModularCraftPartComponent>? part,
        ProtoId<CP14ModularCraftPartPrototype> partProto,
        ProtoId<CP14ModularCraftSlotPrototype> slot)
    {
        if (!start.Comp.FreeSlots.Contains(slot))
            return false;

        var xform = Transform(start);
        if (xform.GridUid != xform.ParentUid)
            return false;

        AddPartToSlot(start, part, partProto, slot);
        return true;
    }

    private void AddPartToSlot(Entity<CP14ModularCraftStartPointComponent> start,
        Entity<CP14ModularCraftPartComponent>? part,
        ProtoId<CP14ModularCraftPartPrototype> partProto,
        ProtoId<CP14ModularCraftSlotPrototype> slot)
    {
        start.Comp.FreeSlots.Remove(slot);
        start.Comp.InstalledParts.Add(partProto);

        var indexedPart = _proto.Index(partProto);
        start.Comp.FreeSlots.AddRange(indexedPart.AddSlots);

        foreach (var modifier in indexedPart.Modifiers)
        {
            modifier.Effect(EntityManager, start, part);
        }

        _item.VisualsChanged(start);
        Dirty(start);
    }

    public void DisassembleModular(EntityUid target)
    {
        if (!TryComp<CP14ModularCraftStartPointComponent>(target, out var modular))
            return;

        var sourceCoord = _transform.GetMapCoordinates(target);

        //Spawn start part
        if (modular.StartProtoPart is not null)
        {
            if (_random.Prob(0.5f)) //TODO: Dehardcode
            {
                var spawned = Spawn(modular.StartProtoPart, sourceCoord);
                _throwing.TryThrow(spawned, _random.NextAngle().ToWorldVec(), 1f);
            }
        }

        //Spawn parts
        foreach (var part in modular.InstalledParts)
        {
            if (!_proto.TryIndex(part, out var indexedPart))
                continue;

            if (_random.Prob(indexedPart.DestroyProb))
                continue;

            var spawned = Spawn(indexedPart.SourcePart, sourceCoord);
            _throwing.TryThrow(spawned, _random.NextAngle().ToWorldVec(), 1f);
        }

        //Delete
        QueueDel(target);
    }
}
