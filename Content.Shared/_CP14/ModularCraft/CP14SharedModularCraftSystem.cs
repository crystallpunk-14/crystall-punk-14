using Content.Shared._CP14.ModularCraft.Components;
using Content.Shared._CP14.ModularCraft.Prototypes;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.ModularCraft;

public abstract class CP14SharedModularCraftSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14ModularCraftStartPointComponent, MapInitEvent>(OnStartPointMapInit);
        SubscribeLocalEvent<CP14ModularCraftPartComponent, AfterInteractEvent>(OnAfterInteractPart);
        SubscribeLocalEvent<CP14ModularCraftPartComponent, GetVerbsEvent<UtilityVerb>>(OnPartGetVerb);
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

    private void OnPartGetVerb(Entity<CP14ModularCraftPartComponent> ent, ref GetVerbsEvent<UtilityVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        if (!TryComp<CP14ModularCraftStartPointComponent>(args.Target, out var starterPoint))
            return;
        Entity<CP14ModularCraftStartPointComponent> starter = (args.Target, starterPoint);

        List<CP14ModularCraftPartPrototype> possiblePart = new();

        //Calculate possible slots
        foreach (var possibleProto in ent.Comp.PossibleParts)
        {
            if (!starterPoint.FreeSlots.Contains(possibleProto.Id))
                continue;

            possiblePart.Add(_proto.Index(possibleProto));
        }

        foreach (var partProto in possiblePart)
        {
            var indexedSlot = _proto.Index(partProto.TargetSlot);
            var verb = new UtilityVerb()
            {
                Text = Loc.GetString("cp14-modular-craft-add-part-verb-text", ("slot", indexedSlot.Name)),
                Category = VerbCategory.CP14ModularCraft,
                Act = () =>
                {
                    if (TryAddPartToSlot(starter, ent, partProto, indexedSlot))
                        QueueDel(ent);
                },
            };
            args.Verbs.Add(verb);
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

    private void OnAfterInteractPart(Entity<CP14ModularCraftPartComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || args.Target is null)
            return;

        if (!TryComp<CP14ModularCraftStartPointComponent>(args.Target, out var starterPoint))
            return;

        AddPartToFirstSlot((args.Target.Value, starterPoint), ent);
        args.Handled = true;
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
}
