using System.Text;
using Content.Server.Cargo.Systems;
using Content.Server.Popups;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared._CP14.Trading.Components;


namespace Content.Server._CP14.Trading;

public sealed class ExaminePriceSystem : EntitySystem
{
    [Dependency] private readonly PricingSystem _price = default!;
    [Dependency] private readonly InventorySystem _invSystem = default!;
    [Dependency] private PopupSystem _popup = default!;


    public override void Initialize()
    {
        SubscribeLocalEvent<MetaDataComponent, ExaminedEvent>(OnExamined);
    }

    private bool IsAbleExamine(EntityUid uid)
    {
        if (_invSystem.TryGetSlotEntity(uid, "eyes", out var huds)
            && HasComp<CP14ExaminePriceComponent>(huds))
        {
            return true;
        }
        else if (HasComp<CP14ExaminePriceComponent>(uid))
        {
            return true;
        }

        return false;
    }

    private void OnExamined(EntityUid eid, MetaDataComponent component, ExaminedEvent args)
    {
        if (!IsAbleExamine(args.Examiner))
        {
            return;
        }

        var price = _price.GetPrice(args.Examined);

        var copper = Math.Round(price % 10);

        var silver = Math.Round(((price - copper) % 100) / 10);

        var gold = Math.Round(((price - (price%100)) % 1000) / 100);

        var plat = Math.Round((price - (price % 1000)) / 1000);

        var sb = new StringBuilder();

        sb.Append(Loc.GetString($"cp14-trading-item-price"));

        if (plat > 0)
            sb.Append(" " + Loc.GetString("cp14-currency-examine-pp", ("coin", plat)));
        if (gold > 0)
            sb.Append(" " + Loc.GetString("cp14-currency-examine-gp", ("coin", gold)));
        if (silver > 0)
            sb.Append(" " + Loc.GetString("cp14-currency-examine-sp", ("coin", silver)));
        if (copper > 0)
            sb.Append(" " + Loc.GetString("cp14-currency-examine-cp", ("coin", copper)));
        if (plat <= 0 && gold <= 0 && silver <= 0 && copper <= 0)
            sb.Append(" " + Loc.GetString("cp14-trading-empty-price"));

        var priceMsg = sb.ToString();

        args.PushMarkup(priceMsg);
    }

}
