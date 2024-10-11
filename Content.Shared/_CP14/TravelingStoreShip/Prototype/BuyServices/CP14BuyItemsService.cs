using System.Text;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.TravelingStoreShip.Prototype.BuyServices;

public sealed partial class CP14BuyItemsService : CP14StoreBuyService
{
    [DataField(required: true)]
    public Dictionary<EntProtoId, int> Product = new();

    public override void Buy(EntityManager entManager, EntityUid station)
    {
        foreach (var pai in Product)
        {
            Logger.Debug($"куплено: {pai.Key} x{pai.Value}");
        }
    }

    public override string? GetDescription(IPrototypeManager prototype, IEntityManager entSys)
    {
        var sb = new StringBuilder();
        sb.Append(Loc.GetString("cp14-store-service-buy-items") + " \n");
        foreach (var pai in Product)
        {
            if (!prototype.TryIndex(pai.Key, out var indexedProto))
                continue;

            sb.Append($"{indexedProto.Name} x{pai.Value} \n");
        }
        return sb.ToString();
    }
}
