using Content.Server.Cargo.Components;
using Content.Server.Item;
using Content.Shared._CP14.ModularCraft;
using Content.Shared._CP14.ModularCraft.Components;
using Content.Shared._CP14.ModularCraft.Prototypes;
using Content.Shared.Throwing;
using Content.Shared.Examine;
using Content.Shared.Materials;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server._CP14.ModularCraft;

public sealed class CP14ModularCraftSystem : CP14SharedModularCraftSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ItemSystem _item = default!;

    [Dependency] private readonly ExamineSystemShared _examine = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14ModularCraftStartPointComponent, MapInitEvent>(OnStartPointMapInit);
        SubscribeLocalEvent<CP14ModularCraftStartPointComponent, CP14ModularCraftAddPartDoAfter>(OnAddedToStart);
        SubscribeLocalEvent<CP14ModularCraftPartComponent, CP14ModularCraftAddPartDoAfter>(OnAddedToPart);
        SubscribeLocalEvent<CP14ModularCraftStartPointComponent, GetVerbsEvent<ExamineVerb>>(OnVerbExamine);
    }

    private void OnVerbExamine(Entity<CP14ModularCraftStartPointComponent> ent, ref GetVerbsEvent<ExamineVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        var markup = GetExamine(ent.Comp);
        _examine.AddDetailedExamineVerb(
            args,
            ent.Comp,
            markup,
            Loc.GetString("cp14-modular-craft-examine"),
            "/Textures/Interface/VerbIcons/settings.svg.192dpi.png");
    }

    private FormattedMessage GetExamine(CP14ModularCraftStartPointComponent comp)
    {
        var msg = new FormattedMessage();

        msg.AddMarkupOrThrow(Loc.GetString("cp14-modular-craft-examine-freeslots"));

        foreach (var slot in comp.FreeSlots)
        {
            if (!_proto.TryIndex(slot, out var slotProto))
                continue;

            msg.AddMarkupOrThrow("\n - " + Loc.GetString(slotProto.Name));
        }

        return msg;
    }

    private void OnAddedToStart(Entity<CP14ModularCraftStartPointComponent> start, ref CP14ModularCraftAddPartDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!TryComp<CP14ModularCraftPartComponent>(args.Used, out var partComp))
            return;

        if (!TryAddPartToFirstSlot(start, (args.Used.Value, partComp)))
            return;

        //TODO: Sound

        args.Handled = true;
    }

    private void OnAddedToPart(Entity<CP14ModularCraftPartComponent> part, ref CP14ModularCraftAddPartDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!TryComp<CP14ModularCraftStartPointComponent>(args.Used, out var startComp))
            return;

        if (!TryAddPartToFirstSlot((args.Used.Value, startComp), part))
            return;

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
                TryAddPartToFirstSlot(ent, detail);
            }
        }
    }

    private bool TryAddPartToFirstSlot(Entity<CP14ModularCraftStartPointComponent> start,
        Entity<CP14ModularCraftPartComponent> part)
    {
        foreach (var partProto in part.Comp.PossibleParts)
        {
            if (!_proto.TryIndex(partProto, out var partIndexed))
                continue;

            if (partIndexed.Slots.Count == 0)
                continue;

            foreach (var slot in partIndexed.Slots)
            {
                if (!start.Comp.FreeSlots.Contains(slot))
                    continue;

                if (TryAddPartToSlot(start, part, partProto, slot))
                {
                    QueueDel(part);
                    return true;
                }
            }
        }

        return false;
    }

    private bool TryAddPartToFirstSlot(Entity<CP14ModularCraftStartPointComponent> start,
        ProtoId<CP14ModularCraftPartPrototype> partProto)
    {
        if (!_proto.TryIndex(partProto, out var partIndexed))
            return false;

        if (partIndexed.Slots.Count == 0)
            return false;

        foreach (var slot in partIndexed.Slots)
        {
            if (!start.Comp.FreeSlots.Contains(slot))
                continue;

            return TryAddPartToSlot(start, null, partProto, slot);
        }

        return false;
    }

    private bool TryAddPartToSlot(Entity<CP14ModularCraftStartPointComponent> start,
        Entity<CP14ModularCraftPartComponent>? part,
        ProtoId<CP14ModularCraftPartPrototype> partProto,
        ProtoId<CP14ModularCraftSlotPrototype> slot)
    {
        if (!start.Comp.FreeSlots.Contains(slot))
            return false;

        //TODO: Size changing broken in gridstorage

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

        if (TryComp<PhysicalCompositionComponent>(part, out var partMaterial))
        {
            var startMaterial = EnsureComp<PhysicalCompositionComponent>(start);

            //Merge materials
            foreach (var (material, count) in partMaterial.MaterialComposition)
            {
                if (startMaterial.MaterialComposition.TryGetValue(material, out var existingCount))
                    startMaterial.MaterialComposition[material] = existingCount + count;
                else
                    startMaterial.MaterialComposition[material] = count;
            }

            //Merge solutions
            foreach (var (sol, count) in partMaterial.ChemicalComposition)
            {
                if (startMaterial.ChemicalComposition.TryGetValue(sol, out var existingCount))
                    startMaterial.ChemicalComposition[sol] = existingCount + count;
                else
                    startMaterial.ChemicalComposition[sol] = count;
            }
        }

        if (TryComp<Shared.Cargo.Components.StaticPriceComponent>(part, out var staticPartPrice))
        {
            var startStaticPrice = EnsureComp<Shared.Cargo.Components.StaticPriceComponent>(start);

            startStaticPrice.Price += staticPartPrice.Price;
        }

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

            if (indexedPart.SourcePart is null)
                continue;

            var spawned = Spawn(indexedPart.SourcePart, sourceCoord);
            _throwing.TryThrow(spawned, _random.NextAngle().ToWorldVec(), 1f);
        }

        //Delete
        QueueDel(target);
    }
}
