using Content.Shared.Examine;
using Content.Shared.Stacks;

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
        var total = GetTotalCurrency(currency, currency.Comp);

        var push = Loc.GetString("cp14-currency-examine-title");
        push += GetPrettyCurrency(total);
        args.PushMarkup(push);
    }

    public string GetPrettyCurrency(int currency)
    {
        var total = currency;

        if (total <= 0)
            return string.Empty;

        var gp = total / 100;
        total %= 100;

        var sp = total / 10;
        total %= 10;

        var cp = total;

        var push = string.Empty;

        if (gp > 0) push += " " + Loc.GetString("cp14-currency-examine-gp", ("coin", gp));
        if (sp > 0) push += " " + Loc.GetString("cp14-currency-examine-sp", ("coin", sp));
        if (cp > 0) push += " " + Loc.GetString("cp14-currency-examine-cp", ("coin", cp));

        return push;
    }

    public int GetTotalCurrency(EntityUid uid, CP14CurrencyComponent? currency = null)
    {
        if (!Resolve(uid, ref currency))
            return 0;

        var total = currency.Currency;

        if (TryComp<StackComponent>(uid, out var stack))
        {
            total *= stack.Count;
        }

        return total;
    }
}
