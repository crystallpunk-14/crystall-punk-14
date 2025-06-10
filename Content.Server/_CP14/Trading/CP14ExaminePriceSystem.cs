using System.Text;
using Content.Server.Cargo.Systems;
using Content.Shared._CP14.Currency;
using Content.Server.Popups;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared._CP14.Trading.Components;


namespace Content.Server._CP14.Trading;

public sealed class ExaminePriceSystem : CP14SharedCurrencySystem
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

        var getPrice = _price.GetPrice(args.Examined);

        var price = Math.Round(getPrice);

        var priceMsg = Loc.GetString("cp14-currency-examine-title");

        priceMsg += GetCurrencyPrettyString((int)price);

        args.PushMarkup(priceMsg);
    }

}
