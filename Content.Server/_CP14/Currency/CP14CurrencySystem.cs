using Content.Server.Popups;
using Content.Shared._CP14.Currency;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Verbs;
using Content.Shared.Whitelist;
using Robust.Shared.Utility;

namespace Content.Server._CP14.Currency;

public sealed partial class CP14CurrencySystem : CP14SharedCurrencySystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14CurrencyComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<CP14CurrencyConverterComponent, ExaminedEvent>(OnConverterExamine);

        SubscribeLocalEvent<CP14CurrencyConverterComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<CP14CurrencyConverterComponent, GetVerbsEvent<Verb>>(OnGetVerb);
    }

    private void OnExamine(Entity<CP14CurrencyComponent> currency, ref ExaminedEvent args)
    {
        var total = GetTotalCurrency(currency, currency.Comp);

        var push = Loc.GetString("cp14-currency-examine-title");
        push += GetCurrencyPrettyString(total);
        args.PushMarkup(push);
    }

    private void OnConverterExamine(Entity<CP14CurrencyConverterComponent> ent, ref ExaminedEvent args)
    {
        var push = $"{Loc.GetString("cp14-currency-converter-examine-title")} {GetCurrencyPrettyString(ent.Comp.Balance)}";
        args.PushMarkup(push);
    }

    private void OnInteractUsing(Entity<CP14CurrencyConverterComponent> ent, ref InteractUsingEvent args)
    {
        if (!TryComp<CP14CurrencyComponent>(args.Used, out var currency))
            return;

        if (ent.Comp.Whitelist is not null && !_whitelist.IsValid(ent.Comp.Whitelist, args.Used))
            return;

        var delta = GetTotalCurrency(args.Used);
        ent.Comp.Balance += delta;
        QueueDel(args.Used);

        _popup.PopupPredicted(Loc.GetString("cp14-currency-converter-insert", ("cash", delta)), ent, args.User);
    }

    private void OnGetVerb(Entity<CP14CurrencyConverterComponent> ent, ref GetVerbsEvent<Verb> args) //So hardcoded...
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        var coord = Transform(ent).Coordinates;
        Verb copperVerb = new()
        {
            Text = Loc.GetString("cp14-currency-converter-get-cp"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/_CP14/Objects/Economy/cp_coin.rsi/coin10.png")),
            Category = VerbCategory.CP14CurrencyConvert,
            Priority = 1,
            CloseMenu = false,
            Act = () =>
            {
                if (ent.Comp.Balance < 1)
                    return;

                ent.Comp.Balance -= 1;
                GenerateMoney("CP14CopperCoin1", 1, 1, coord, out var remainder);
            },
        };
        args.Verbs.Add(copperVerb);
        Verb silverVerb = new()
        {
            Text = Loc.GetString("cp14-currency-converter-get-sp"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/_CP14/Objects/Economy/sp_coin.rsi/coin10.png")),
            Category = VerbCategory.CP14CurrencyConvert,
            Priority = 2,
            CloseMenu = false,
            Act = () =>
            {
                if (ent.Comp.Balance < 10)
                    return;

                ent.Comp.Balance -= 10;
                GenerateMoney("CP14SilverCoin1", 10, 10, coord, out var remainder);
            },
        };
        args.Verbs.Add(silverVerb);
        Verb goldVerb = new()
        {
            Text = Loc.GetString("cp14-currency-converter-get-gp"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/_CP14/Objects/Economy/gp_coin.rsi/coin10.png")),
            Category = VerbCategory.CP14CurrencyConvert,
            Priority = 3,
            CloseMenu = false,
            Act = () =>
            {
                if (ent.Comp.Balance < 100)
                    return;

                ent.Comp.Balance -= 100;
                GenerateMoney("CP14GoldCoin1", 100, 100, coord, out var remainder);
            },
        };
        args.Verbs.Add(goldVerb);
        Verb platinumVerb = new()
        {
            Text = Loc.GetString("cp14-currency-converter-get-pp"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/_CP14/Objects/Economy/pp_coin.rsi/coin10.png")),
            Category = VerbCategory.CP14CurrencyConvert,
            Priority = 4,
            CloseMenu = false,
            Act = () =>
            {
                if (ent.Comp.Balance < 1000)
                    return;

                ent.Comp.Balance -= 1000;
                GenerateMoney("CP14PlatinumCoin1", 1000, 1000, coord, out var remainder);
            },
        };
        args.Verbs.Add(platinumVerb);
    }
}
