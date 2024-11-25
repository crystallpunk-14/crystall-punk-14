using Content.Shared._CP14.ModularCraft;
using Content.Shared._CP14.ModularCraft.Components;
using Content.Shared._CP14.ModularCraft.Prototypes;
using Content.Shared.Interaction;
using Content.Shared.Verbs;
using FastAccessors;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._CP14.ModularCraft;

public sealed class CP14ModularCraftSystem : CP14SharedModularCraftSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14ModularCraftStartPointComponent, MapInitEvent>(OnStartPointMapInit);

        SubscribeLocalEvent<CP14ModularCraftPartComponent, GetVerbsEvent<UtilityVerb>>(OnPartGetVerb);
        SubscribeLocalEvent<CP14ModularCraftPartComponent, AfterInteractEvent>(OnAfterInteractPart);
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
        foreach (var possibleProto in ent.Comp.PossiblePartProto)
        {
            if (!starterPoint.FreeSlots.Contains(possibleProto.Id))
                continue;

            possiblePart.Add(_proto.Index(possibleProto));
        }

        foreach (var part in possiblePart)
        {
            var indexedSlot = _proto.Index(part.TargetSlot);
            var verb = new UtilityVerb()
            {
                Text = Loc.GetString("cp14-modular-craft-add-part-verb-text", ("slot", indexedSlot.Name)),
                Category = VerbCategory.CP14ModularCraft,
                Act = () =>
                {
                    if (TryAddPartToSlot(starter, part, indexedSlot))
                        QueueDel(ent);
                },
            };
            args.Verbs.Add(verb);
        }
    }

    private void OnStartPointMapInit(Entity<CP14ModularCraftStartPointComponent> ent, ref MapInitEvent args)
    {
        foreach (var startSlot in ent.Comp.StartSlots)
        {
            ent.Comp.FreeSlots.Add(startSlot);
        }
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

    private void AddPartToFirstSlot(Entity<CP14ModularCraftStartPointComponent> start,
        Entity<CP14ModularCraftPartComponent> part)
    {
        foreach (var partProto in part.Comp.PossiblePartProto)
        {
            if (!_proto.TryIndex(partProto, out var partIndexed))
                continue;

            if (!start.Comp.FreeSlots.Contains(partIndexed.TargetSlot))
                continue;

            if (TryAddPartToSlot(start, partProto, partIndexed.TargetSlot))
            {
                QueueDel(part);
            }

            return;
        }
    }

    private bool TryAddPartToSlot(Entity<CP14ModularCraftStartPointComponent> start,
        ProtoId<CP14ModularCraftPartPrototype> part,
        ProtoId<CP14ModularCraftSlotPrototype> slot)
    {
        if (!start.Comp.FreeSlots.Contains(slot))
            return false;

        start.Comp.FreeSlots.Remove(slot);
        start.Comp.InstalledParts.Add(part);

        var indexedPart = _proto.Index(part);
        start.Comp.FreeSlots.AddRange(indexedPart.AddSlots);

        return true;
    }
}
