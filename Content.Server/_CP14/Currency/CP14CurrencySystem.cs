using Content.Server.Popups;
using Content.Server.Stack;
using Content.Server.Storage.Components;
using Content.Shared._CP14.Currency;
using Content.Shared.Examine;
using Content.Shared.Stacks;
using Content.Shared.Storage;
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

    public override void Initialize()
    {
        base.Initialize();

        InitializeConverter();

        SubscribeLocalEvent<CP14CurrencyExaminableComponent, ExaminedEvent>(OnExamine);

        SubscribeLocalEvent<CP14CurrencyComponent, CP14GetCurrencyEvent>(OnGetCurrency);
        //SubscribeLocalEvent<EntityStorageComponent, CP14GetCurrencyEvent>(OnEntityStorageGetCurrency);
        //SubscribeLocalEvent<StorageComponent, CP14GetCurrencyEvent>(OnStorageGetCurrency);
    }

    private void OnGetCurrency(Entity<CP14CurrencyComponent> ent, ref CP14GetCurrencyEvent args)
    {
        var total = ent.Comp.Currency;
        if (TryComp<StackComponent>(ent, out var stack))
        {
            total *= stack.Count;
        }

        args.Currency += total;
    }

    //private void OnEntityStorageGetCurrency(Entity<EntityStorageComponent> ent, ref CP14GetCurrencyEvent args)
    //{
    //    var total = 0;
    //    foreach (var entity in ent.Comp.Contents.ContainedEntities)
    //    {
    //        total += GetTotalCurrency(entity);
    //    }
//
    //    args.Currency += total;
    //}
//
    //private void OnStorageGetCurrency(Entity<StorageComponent> ent, ref CP14GetCurrencyEvent args)
    //{
    //    var total = 0;
    //    foreach (var entity in ent.Comp.StoredItems)
    //    {
    //        total += GetTotalCurrency(entity.Key);
    //    }
//
    //    args.Currency += total;
    //}

    private void OnExamine(Entity<CP14CurrencyExaminableComponent> currency, ref ExaminedEvent args)
    {
        var total = GetTotalCurrency(currency);

        var push = Loc.GetString("cp14-currency-examine-title");
        push += GetCurrencyPrettyString(total);
        args.PushMarkup(push);
    }
}
