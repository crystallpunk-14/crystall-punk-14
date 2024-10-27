using System.Text;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.TravelingStoreShip.Prototype.BuyServices;

public sealed partial class CP14BuyItemsService : CP14StoreBuyService
{
    [DataField(required: true)]
    public Dictionary<EntProtoId, int> Product = new();

    public override void Buy(EntityManager entManager, Entity<CP14StationTravelingStoreShipTargetComponent> station)
    {
        foreach (var (protoId, count) in Product)
        {
            for (var i = 0; i < count; i++)
            {
                station.Comp.BuyedQueue.Enqueue(protoId);
            }
        }
    }

    public override string? GetDescription(IPrototypeManager prototype, IEntityManager entSys)
    {
        var sb = new StringBuilder();
        sb.Append(Loc.GetString("cp14-store-service-buy-items") + " \n");
        foreach (var (protoId, count) in Product)
        {
            if (!prototype.TryIndex(protoId, out var indexedProto))
                continue;

            sb.Append($"{indexedProto.Name} x{count} \n");
        }
        return sb.ToString();
    }
}
