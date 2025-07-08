using Content.Server._CP14.Currency;
using Content.Server._CP14.Trading;
using Content.Server.Cargo.Systems;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Salary;

public sealed partial class CP14SalarySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly PricingSystem _price = default!;
    [Dependency] private readonly CP14CurrencySystem _cp14Currency = default!;
    [Dependency] private readonly CP14StationEconomySystem _economy = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14SalaryPairollComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<CP14SalaryPairollComponent, InteractHandEvent>(OnInteract);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14SalaryCounterComponent>();
        while (query.MoveNext(out var ent, out var counter))
        {
            if (_timing.CurTime < counter.NextSalaryTime)
                continue;
            counter.NextSalaryTime = _timing.CurTime + counter.Frequency;

            counter.UnpaidSalary += counter.Salary;
        }
    }

    private void OnExamined(Entity<CP14SalaryPairollComponent> ent, ref ExaminedEvent args)
    {
        if (!TryComp<CP14SalaryCounterComponent>(args.Examiner, out var counter))
        {
            args.PushMarkup(Loc.GetString("cp14-salary-payroll-examine-unsupported-job"));
            return;
        }

        if (counter.UnpaidSalary <= 0)
        {
            args.PushMarkup(Loc.GetString("cp14-salary-payroll-examine-empty"));
        }
        else
        {
            args.PushMarkup(Loc.GetString("cp14-salary-payroll-examine", ("count", _cp14Currency.GetCurrencyPrettyString(counter.UnpaidSalary))));
        }

        //Timer
        var remainingToSalaryTime = counter.NextSalaryTime - _timing.CurTime;
        //time in format mm:ss
        var minutes = (int)remainingToSalaryTime.TotalMinutes;
        var seconds = remainingToSalaryTime.Seconds;

        args.PushMarkup(Loc.GetString("cp14-salary-payroll-examine-timer", ("time", $"{minutes:D2}:{seconds:D2}")));
    }

    private void OnInteract(Entity<CP14SalaryPairollComponent> ent, ref InteractHandEvent args)
    {
        if (!TryComp<CP14SalaryCounterComponent>(args.User, out var counter))
        {
            _popup.PopupEntity(Loc.GetString("cp14-salary-payroll-examine-unsupported-job"), args.User, args.User);
            return;
        }

        if (counter.UnpaidSalary <= 0)
        {
            _popup.PopupEntity(Loc.GetString("cp14-salary-payroll-examine-empty"), args.User, args.User);
            return;
        }

        _audio.PlayPvs(ent.Comp.BuySound, Transform(ent).Coordinates);
        SpawnAtPosition(ent.Comp.BuyVisual, Transform(ent).Coordinates);

        _cp14Currency.GenerateMoney(counter.UnpaidSalary, Transform(ent).Coordinates);
        counter.UnpaidSalary = 0;
    }
}
