using System.Text;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.TravelingStoreShip.Prototype.SellServices;

public sealed partial class CP14SellEntitiesService : CP14StoreSellService
{
    [DataField(required: true)]
    public Dictionary<EntProtoId, int> Product = new();

    public override bool TrySell(EntityManager entManager, EntityUid station)
    {
        foreach (var pai in Product)
        {
            Logger.Debug($"продано: {pai.Key} x{pai.Value}");
        }

        return true;
    }

    public override string? GetDescription(IPrototypeManager prototype, IEntityManager entSys)
    {
        var sb = new StringBuilder();
        sb.Append(Loc.GetString("cp14-store-service-sell-entities") + " \n");
        foreach (var pai in Product)
        {
            if (!prototype.TryIndex(pai.Key, out var indexedProto))
                continue;

            sb.Append($"{indexedProto.Name} x{pai.Value} \n");
        }
        return sb.ToString();
    }
}
