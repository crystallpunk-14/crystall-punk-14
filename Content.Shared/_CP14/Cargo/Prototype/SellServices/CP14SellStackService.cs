using Content.Shared.Stacks;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Cargo.Prototype.SellServices;

public sealed partial class CP14SellStackService : CP14StoreSellService
{
    [DataField(required: true)]
    public ProtoId<StackPrototype> StackId = new();

    [DataField(required: true)]
    public int Count = 1;

    public override bool TrySell(EntityManager entManager, HashSet<EntityUid> entities)
    {
        var stackSystem = entManager.System<SharedStackSystem>();

        Dictionary<Entity<StackComponent>, int> suitable = new();

        int needCount = Count;
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
}
