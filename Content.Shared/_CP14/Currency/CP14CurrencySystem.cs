using Content.Shared.Examine;

namespace Content.Shared._CP14.Currency;

public sealed partial class CP14CurrencySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14CurrencyComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(Entity<CP14CurrencyComponent> currency, ref ExaminedEvent args)
    {
        var total = currency.Comp.Currency;

        if (total <= 0)
            return;

        var gp = total / 100;
        total %= 100;

        var sp = total / 10;
        total %= 10;

        var cp = total;

        var push = Loc.GetString("cp14-currency-examine-title");

        if (gp > 0) push += " " + Loc.GetString("cp14-currency-examine-gp", ("coin", gp));
        if (sp > 0) push += " " + Loc.GetString("cp14-currency-examine-sp", ("coin", sp));
        if (cp > 0) push += " " + Loc.GetString("cp14-currency-examine-cp", ("coin", cp));

        args.PushMarkup(push);
    }
}
