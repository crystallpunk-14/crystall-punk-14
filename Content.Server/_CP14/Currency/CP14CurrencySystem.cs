using Content.Server.Cargo.Systems;
using Content.Server.Popups;
using Content.Server.Stack;
using Content.Shared._CP14.Currency;
using Content.Shared.Examine;
using Content.Shared.Whitelist;
using Robust.Server.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Currency;

public sealed partial class CP14CurrencySystem : CP14SharedCurrencySystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly StackSystem _stack = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly PricingSystem _price = default!;

    public override void Initialize()
    {
        base.Initialize();

        InitializeConverter();

        SubscribeLocalEvent<CP14CurrencyExaminableComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(Entity<CP14CurrencyExaminableComponent> currency, ref ExaminedEvent args)
    {
        var price = _price.GetPrice(currency);
        var push = Loc.GetString("cp14-currency-examine-title");
        push += GetCurrencyPrettyString((int)price);
        args.PushMarkup(push);
    }
}
