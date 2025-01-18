using System.Text;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Cargo.Prototype.BuyServices;

public sealed partial class CP14BuyItemsService : CP14StoreBuyService
{
    [DataField(required: true)]
    public Dictionary<EntProtoId, int> Product = new();

    public override void Buy(EntityManager entManager, IPrototypeManager prototype, Entity<CP14StationTravelingStoreShipTargetComponent> station)
    {
        foreach (var (protoId, count) in Product)
        {
            for (var i = 0; i < count; i++)
            {
                station.Comp.BuyedQueue.Enqueue(protoId);
            }
        }
    }
}
