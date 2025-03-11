using Content.Server.Popups;
using Content.Server.Stack;
using Content.Server.Storage.Components;
using Content.Shared._CP14.Currency;
using Content.Shared.Examine;
using Content.Shared.Stacks;
using Content.Shared.Storage;
using Content.Shared.Whitelist;
using Robust.Server.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Currency;

public sealed partial class CP14CurrencySystem : CP14SharedCurrencySystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly StackSystem _stack = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        InitializeConverter();

        SubscribeLocalEvent<CP14CurrencyExaminableComponent, ExaminedEvent>(OnExamine);

        SubscribeLocalEvent<CP14CurrencyComponent, CP14GetCurrencyEvent>(OnGetCurrency);
        SubscribeLocalEvent<ContainerManagerComponent, CP14GetCurrencyEvent>(OnContainerGetCurrency);
    }

    private void OnGetCurrency(Entity<CP14CurrencyComponent> ent, ref CP14GetCurrencyEvent args)
    {
        if (args.CheckedEntities.Contains(ent))
            return;

        var total = ent.Comp.Currency;
        if (TryComp<StackComponent>(ent, out var stack))
        {
            total *= stack.Count;
        }

        args.Currency += total;
        args.CheckedEntities.Add(ent);
    }

    private void OnContainerGetCurrency(Entity<ContainerManagerComponent> ent, ref CP14GetCurrencyEvent args)
    {
        var total = 0;
        foreach (var container in ent.Comp.Containers.Values)
        {
            foreach (var containedEnt in container.ContainedEntities)
            {
                total += GetTotalCurrencyRecursive(containedEnt);
            }
        }

        args.Currency += total;
    }

    private void OnExamine(Entity<CP14CurrencyExaminableComponent> currency, ref ExaminedEvent args)
    {
        var total = GetTotalCurrencyRecursive(currency);

        var push = Loc.GetString("cp14-currency-examine-title");
        push += GetCurrencyPrettyString(total);
        args.PushMarkup(push);
    }
}
