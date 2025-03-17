using Content.Shared._CP14.Cargo;
using Content.Shared._CP14.Cargo.Prototype;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server._CP14.Cargo.BuyServices;

public sealed partial class CP14RerollSpecialPositionsService : CP14StoreBuyService
{
    [DataField] public int RerollBuy;
    [DataField] public int RerollSell;

    public override void Buy(EntityManager entManager, IPrototypeManager prototype, Entity<CP14TradingPortalComponent> portal)
    {
        var randomSystem = IoCManager.Resolve<IRobustRandom>();
        var cargoSystem = entManager.System<CP14CargoSystem>();

        if (RerollBuy > 0)
        {
            var removed = 0;
            for (var i = 0; i < RerollBuy; i++)
            {
                if (portal.Comp.CurrentSpecialBuyPositions.Count == 0)
                    break;
                removed++;
                var position = randomSystem.Pick(portal.Comp.CurrentSpecialBuyPositions);
                portal.Comp.CurrentSpecialBuyPositions.Remove(position.Key);
            }

            cargoSystem.AddRandomBuySpecialPosition(portal, removed);
        }

        if (RerollSell > 0)
        {
            var removed = 0;
            for (var i = 0; i < RerollSell; i++)
            {
                if (portal.Comp.CurrentSpecialSellPositions.Count == 0)
                    break;
                removed++;
                var position = randomSystem.Pick(portal.Comp.CurrentSpecialSellPositions);
                portal.Comp.CurrentSpecialSellPositions.Remove(position.Key);
            }

            cargoSystem.AddRandomSellSpecialPosition(portal, removed);
        }
    }

    public override string GetName(IPrototypeManager protoMan)
    {
        return string.Empty;
    }

    public override EntProtoId? GetEntityView(IPrototypeManager protoManager)
    {
        return null;
    }

    public override SpriteSpecifier? GetTexture(IPrototypeManager protoManager)
    {
        return null;
    }
}
