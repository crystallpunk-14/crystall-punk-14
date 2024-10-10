using Content.Shared._CP14.TravelingStoreShip;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.TravelingStoreShip.Services;

public sealed partial class CP14BuyItemsService : CP14StoreBuyService
{
    [DataField(required: true)]
    public Dictionary<EntProtoId, int> Product = new();

    public override void Effect(EntityManager entManager, EntityUid station)
    {
        foreach (var pai in Product)
        {
            Logger.Debug($"куплено: {pai.Key} x{pai.Value}");
        }
    }

    public override string? GetDescription(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return string.Empty;
    }
}
