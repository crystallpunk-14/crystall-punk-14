using Content.Shared._CP14.Currency;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Stacks;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._CP14.Currency;

public sealed partial class CP14CurrencySystem
{
    private void InitializeConverter()
    {
        SubscribeLocalEvent<CP14CurrencyConverterComponent, GetVerbsEvent<Verb>>(OnGetVerb);
        SubscribeLocalEvent<CP14CurrencyConverterComponent, ExaminedEvent>(OnConverterExamine);
        SubscribeLocalEvent<CP14CurrencyConverterComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnGetVerb(Entity<CP14CurrencyConverterComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        var transform = Transform(ent);
        var coord = transform.Coordinates.Offset(transform.LocalRotation.RotateVec(ent.Comp.SpawnOffset));
        Verb copperVerb = new()
        {
            Text = Loc.GetString("cp14-currency-converter-get-cp"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/_CP14/Objects/Economy/cp_coin.rsi/coin10.png")),
            Category = VerbCategory.CP14CurrencyConvert,
            Priority = 1,
            CloseMenu = false,
            Act = () =>
            {
                if (ent.Comp.Balance < CP.Value)
                    return;

                ent.Comp.Balance -= CP.Value;

                var newEnt = Spawn(CP.Key, coord);
                _stack.TryMergeToContacts(newEnt);
                _audio.PlayPvs(ent.Comp.InsertSound, ent, AudioParams.Default.WithMaxDistance(3).WithPitchScale(0.9f));
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
                if (ent.Comp.Balance < SP.Value)
                    return;

                ent.Comp.Balance -= SP.Value;
                var newEnt = Spawn(SP.Key, coord);
                _stack.TryMergeToContacts(newEnt);
                _audio.PlayPvs(ent.Comp.InsertSound, ent, AudioParams.Default.WithMaxDistance(3).WithPitchScale(1.1f));
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
                if (ent.Comp.Balance < GP.Value)
                    return;

                ent.Comp.Balance -= GP.Value;
                var newEnt = Spawn(GP.Key, coord);
                _stack.TryMergeToContacts(newEnt);
                _audio.PlayPvs(ent.Comp.InsertSound, ent, AudioParams.Default.WithMaxDistance(3).WithPitchScale(1.3f));
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
                if (ent.Comp.Balance < PP.Value)
                    return;

                ent.Comp.Balance -= PP.Value;
                var newEnt = Spawn(PP.Key, coord);
                _stack.TryMergeToContacts(newEnt);
                _audio.PlayPvs(ent.Comp.InsertSound, ent, AudioParams.Default.WithMaxDistance(3).WithPitchScale(1.5f));
            },
        };
        args.Verbs.Add(platinumVerb);
    }


    private void OnConverterExamine(Entity<CP14CurrencyConverterComponent> ent, ref ExaminedEvent args)
    {
        var push =
            $"{Loc.GetString("cp14-currency-converter-examine-title")} {GetCurrencyPrettyString(ent.Comp.Balance)}";
        args.PushMarkup(push);
    }

    private void OnInteractUsing(Entity<CP14CurrencyConverterComponent> ent, ref InteractUsingEvent args)
    {
        if (!TryComp<CP14CurrencyComponent>(args.Used, out var currency))
            return;

        if (ent.Comp.Whitelist is not null && !_whitelist.IsValid(ent.Comp.Whitelist, args.Used))
            return;

        var delta = GetTotalCurrencyRecursive(args.Used);
        ent.Comp.Balance += delta;
        QueueDel(args.Used);

        _popup.PopupEntity(Loc.GetString("cp14-currency-converter-insert", ("cash", delta)), ent, args.User);
        _audio.PlayPvs(ent.Comp.InsertSound, ent, AudioParams.Default.WithMaxDistance(3));
    }

    public HashSet<EntityUid> GenerateMoney(EntProtoId currencyType,
        int target,
        EntityCoordinates coordinates,
        out int remainder)
    {
        remainder = target;
        HashSet<EntityUid> spawns = new();

        if (!_proto.TryIndex(currencyType, out var indexedCurrency))
            return spawns;

        var ent = Spawn(currencyType, coordinates);
        if (ProcessEntity(ent, ref remainder, spawns))
            return spawns;

        while (remainder > 0)
        {
            var newEnt = Spawn(currencyType, coordinates);
            if (ProcessEntity(newEnt, ref remainder, spawns))
                break;
        }

        return spawns;
    }

    public HashSet<EntityUid> GenerateMoney(
        int target,
        EntityCoordinates coordinates)
    {
        HashSet<EntityUid> coins = new();
        var balance = target;
        //PP
        if (balance > 0)
        {
            var ppCoin = GenerateMoney(PP.Key, balance, coordinates, out var remainder);
            balance = remainder;
            foreach (var pp in ppCoin)
            {
                coins.Add(pp);
            }
        }
        //GP
        if (balance > 0)
        {
            var gpCoin = GenerateMoney(GP.Key, balance, coordinates, out var remainder);
            balance = remainder;
            foreach (var gp in gpCoin)
            {
                coins.Add(gp);
            }
        }
        //SP
        if (balance > 0)
        {
            var spCoin = GenerateMoney(SP.Key, balance, coordinates, out var remainder);
            balance = remainder;
            foreach (var sp in spCoin)
            {
                coins.Add(sp);
            }
        }
        //CP
        if (balance > 0)
        {
            var cpCoin = GenerateMoney(CP.Key, balance, coordinates, out var remainder);
            balance = remainder;
            foreach (var cp in cpCoin)
            {
                coins.Add(cp);
            }
        }

        return coins;
    }

    private bool ProcessEntity(EntityUid ent, ref int remainder, HashSet<EntityUid> spawns)
    {
        var singleCurrency = GetTotalCurrencyRecursive(ent);

        if (singleCurrency > remainder)
        {
            QueueDel(ent);
            return true;
        }

        spawns.Add(ent);
        remainder -= singleCurrency;

        if (TryComp<StackComponent>(ent, out var stack) &&
            _proto.TryIndex<StackPrototype>(stack.StackTypeId, out var indexedStack))
        {
            AdjustStack(ent, stack, indexedStack, singleCurrency, ref remainder);
        }

        return false;
    }

    private void AdjustStack(EntityUid ent,
        StackComponent stack,
        StackPrototype stackProto,
        float singleCurrency,
        ref int remainder)
    {
        var singleStackCurrency = singleCurrency / stack.Count;
        var stackLeftSpace = stackProto.MaxCount - stack.Count;

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
