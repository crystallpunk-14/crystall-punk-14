using System.Text;
using Content.Shared.Stacks;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
namespace Content.Shared._CP14.Currency;

public partial class CP14SharedCurrencySystem : EntitySystem
{
    [Dependency] private readonly SharedStackSystem _stack = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public string GetCurrencyPrettyString(int currency)
    {
        var total = currency;

        if (total <= 0)
            return string.Empty;

        var gp = total / 100;
        total %= 100;

        var sp = total / 10;
        total %= 10;

        var cp = total;

        var sb = new StringBuilder();

        if (gp > 0) sb.Append( " " + Loc.GetString("cp14-currency-examine-gp", ("coin", gp)));
        if (sp > 0) sb.Append( " " + Loc.GetString("cp14-currency-examine-sp", ("coin", sp)));
        if (cp > 0) sb.Append( " " + Loc.GetString("cp14-currency-examine-cp", ("coin", cp)));

        return sb.ToString();
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
