using Content.Server.Popups;
using Content.Server.Stack;
using Content.Shared._CP14.Currency;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Verbs;
using Content.Shared.Whitelist;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._CP14.Currency;

public sealed partial class CP14CurrencySystem : CP14SharedCurrencySystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly StackSystem _stack = default!;
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
                var cost = 1;
                if (ent.Comp.Balance < cost)
                    return;

                ent.Comp.Balance -= cost;
                GenerateMoney(CP, cost, cost, coord);
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
                var cost = 10;
                if (ent.Comp.Balance < cost)
                    return;

                ent.Comp.Balance -= cost;
                GenerateMoney(SP, cost, cost, coord);
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
                var cost = 100;
                if (ent.Comp.Balance < cost)
                    return;

                ent.Comp.Balance -= cost;
                GenerateMoney(GP, cost, cost, coord);
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
                var cost = 1000;
                if (ent.Comp.Balance < cost)
                    return;

                ent.Comp.Balance -= cost;
                GenerateMoney(PP, cost, cost, coord);
            },
        };
        args.Verbs.Add(platinumVerb);
    }


    public HashSet<EntityUid> GenerateMoney(EntProtoId currencyType, int target, int perSpawn, EntityCoordinates coordinates)
    {
        return GenerateMoney(currencyType, target, perSpawn, coordinates, out _);
    }

    public HashSet<EntityUid> GenerateMoney(EntProtoId currencyType, int target, int perSpawn, EntityCoordinates coordinates, out int remainder)
    {
        remainder = target;
        HashSet<EntityUid> spawns = new();

        while (remainder > 0)
        {
            if (remainder < perSpawn)
                return spawns;

            var newEnt = Spawn(currencyType, coordinates);
            _stack.TryMergeToContacts(newEnt);
            remainder -= perSpawn;
        }

        return spawns;
    }
}
