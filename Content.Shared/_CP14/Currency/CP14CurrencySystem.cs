using System.Text;
using Content.Shared.Examine;
using Content.Shared.Stacks;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._CP14.Currency;

public sealed partial class CP14CurrencySystem : EntitySystem
{
    [Dependency] private readonly SharedStackSystem _stack = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14CurrencyComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(Entity<CP14CurrencyComponent> currency, ref ExaminedEvent args)
    {
        var total = GetTotalCurrency(currency, currency.Comp);

        var push = Loc.GetString("cp14-currency-examine-title");
        push += GetCurrencyPrettyString(total);
        args.PushMarkup(push);
    }

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

    public HashSet<EntityUid> GenerateMoney(EntProtoId currencyType, int target, out int remainder)
    {
        remainder = target;
        HashSet<EntityUid> spawns = new();

        if (!_proto.TryIndex(currencyType, out var indexedCurrency))
            return spawns;

        var ent = Spawn(currencyType, MapCoordinates.Nullspace);
        if (ProcessEntity(ent, ref remainder, spawns))
            return spawns;

        while (remainder > 0)
        {
            var newEnt = Spawn(currencyType, MapCoordinates.Nullspace);
            if (ProcessEntity(newEnt, ref remainder, spawns))
                break;
        }

        return spawns;
    }

    private bool ProcessEntity(EntityUid ent, ref int remainder, HashSet<EntityUid> spawns)
    {
        var singleCurrency = GetTotalCurrency(ent);

        if (singleCurrency > remainder)
        {
            QueueDel(ent);
            return true;
        }

        spawns.Add(ent);
        remainder -= singleCurrency;

        if (TryComp<StackComponent>(ent, out var stack))
        {
            AdjustStack(ent, stack, singleCurrency, ref remainder);
        }

        return false;
    }

    private void AdjustStack(EntityUid ent, StackComponent stack, float singleCurrency, ref int remainder)
    {
        var singleStackCurrency = singleCurrency / stack.Count;
        var stackLeftSpace = stack.MaxCountOverride - stack.Count;

        if (stackLeftSpace is not null)
        {
            var addedStack = MathF.Min((float)stackLeftSpace, MathF.Floor(remainder / singleStackCurrency));

            if (addedStack > 0)
            {
                _stack.SetCount(ent, stack.Count + (int)addedStack);
                remainder -= (int)(addedStack * singleStackCurrency);
            }
        }
    }
}
