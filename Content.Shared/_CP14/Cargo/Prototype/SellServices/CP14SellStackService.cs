using Content.Shared.Stacks;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Cargo.Prototype.SellServices;

public sealed partial class CP14SellStackService : CP14StoreSellService
{
    [DataField(required: true)]
    public ProtoId<StackPrototype> StackId;

    [DataField(required: true)]
    public int Count = 1;

    public override bool TrySell(EntityManager entManager, HashSet<EntityUid> entities)
    {
        var stackSystem = entManager.System<SharedStackSystem>();

        Dictionary<Entity<StackComponent>, int> suitable = new();

        var needCount = Count;
        foreach (var ent in entities)
        {
            if (needCount <= 0)
                break;

            if (!entManager.TryGetComponent<StackComponent>(ent, out var stack) || stack.StackTypeId != StackId.Id)
                continue;

            var consumed = Math.Min(needCount, stack.Count);
            suitable.Add((ent,stack), consumed);
            needCount -= consumed;
        }

        if (needCount > 0)
            return false;

        foreach (var selledEnt in suitable)
        {
            if (selledEnt.Key.Comp.Count == selledEnt.Value)
            {
                entities.Remove(selledEnt.Key);
                entManager.QueueDeleteEntity(selledEnt.Key);
            }
            else
            {
                stackSystem.Use(selledEnt.Key, selledEnt.Value);
            }
        }

        return true;
    }

    public override string GetName(IPrototypeManager protoMan)
    {
        if (!protoMan.TryIndex(StackId, out var proto))
            return ":3";

        return $"{Loc.GetString(proto.Name)} x{Count}";
    }

    public override EntProtoId? GetEntityView(IPrototypeManager protoManager)
    {
        return null;
    }

    public override SpriteSpecifier? GetTexture(IPrototypeManager protoManager)
    {
        if (!protoManager.TryIndex(StackId, out var proto))
            return null;

        return proto.Icon;
    }
}
