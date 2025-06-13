using System.Text;
using Content.Server.Cargo.Systems;
using Content.Shared._CP14.Currency;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Tag;
using Content.Shared._CP14.Trading.Components;


namespace Content.Server._CP14.Trading;

public sealed class CP14PriceScannerSystem : EntitySystem
{
    [Dependency] private readonly PricingSystem _price = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly InventorySystem _invSystem = default!;
    [Dependency] private readonly CP14SharedCurrencySystem _currency = default!;


    public override void Initialize()
    {
        SubscribeLocalEvent<MetaDataComponent, ExaminedEvent>(OnExamined);
    }

    private bool IsAbleExamine(EntityUid uid)
    {
        if (_invSystem.TryGetSlotEntity(uid, "eyes", out var huds)
            && HasComp<CP14PriceScannerComponent>(huds))
        {
            return true;
        }
        else if (HasComp<CP14PriceScannerComponent>(uid))
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
        else if (_tag.HasTag(args.Examined, "CP14Coin"))
        {
            return;
        }

        var getPrice = _price.GetPrice(args.Examined);

        var price = Math.Round(getPrice);

        var priceMsg = Loc.GetString("cp14-currency-examine-title");

        priceMsg += _currency.GetCurrencyPrettyString((int)price);

        args.PushMarkup(priceMsg);
    }

}
