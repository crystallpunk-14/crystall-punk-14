using System.Text;
using Content.Server._CP14.Cargo;
using Content.Server._CP14.Currency;
using Content.Server.Station.Events;
using Content.Shared.Paper;
using Content.Shared.Station.Components;
using Content.Shared.Storage.EntitySystems;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._CP14.RoleSalary;

/// <summary>
/// A system that periodically sends paychecks to certain roles through the cargo ship system
/// </summary>
public sealed partial class CP14SalarySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly CP14CargoSystem _cargo = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly PaperSystem _paper = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly CP14CurrencySystem _currency = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14StationSalaryComponent, StationPostInitEvent>(OnStationPostInit);
        SubscribeLocalEvent<CP14SalarySpawnerComponent, MapInitEvent>(OnSalaryInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        //var query = EntityQueryEnumerator<CP14StationSalaryComponent, CP14StationTravelingStoreShipTargetComponent>();
        //while (query.MoveNext(out var uid, out var salary, out var store))
        //{
        //    if (_timing.CurTime < salary.NextSalaryTime)
        //        continue;
//
        //    salary.NextSalaryTime = _timing.CurTime + salary.SalaryFrequency;
        //    _cargo.AddBuyQueue((uid, store), new List<EntProtoId> {salary.SalaryProto});
        //}
    }

    private void OnSalaryInit(Entity<CP14SalarySpawnerComponent> ent, ref MapInitEvent args)
    {
        GenerateSalary(ent);
        QueueDel(ent);
    }

    private void GenerateSalary(Entity<CP14SalarySpawnerComponent> ent)
    {
        //Hardcode warning! ^-^
        var xform = Transform(ent);

        //First we need found a station
        if (!TryComp<StationMemberComponent>(xform.GridUid, out var member))
            return;

        if (!TryComp<CP14StationSalaryComponent>(member.Station, out var stationSalaryComponent))
            return;

        var paper = Spawn("CP14Paper"); //TODO Special named paper
        _transform.PlaceNextTo(paper, (ent, xform));
        if (TryComp<PaperComponent>(paper, out var paperComp))
        {
            paperComp.Content = GenerateSalaryText((member.Station, stationSalaryComponent)) ?? "";
            _paper.TryStamp((paper, paperComp),
                new StampDisplayInfo
                {
                    StampedColor = Color.Red,
                    StampedName = Loc.GetString("cp14-stamp-salary"),
                },
                "red_on_paper");
        }

        var wallet = Spawn("CP14Wallet");
        _transform.PlaceNextTo(wallet, (ent, xform));

        foreach (var salary in stationSalaryComponent.Salary)
        {
            var coins = _currency.GenerateMoney(salary.Value, xform.Coordinates);
            foreach (var coin in coins)
            {
                _storage.Insert(wallet, coin, out _);
            }
        }
    }

    private string? GenerateSalaryText(Entity<CP14StationSalaryComponent> station)
    {
        var sb = new StringBuilder();

        sb.Append(Loc.GetString("cp14-salary-title") + "\n");
        foreach (var salary in station.Comp.Salary)
        {
            sb.Append("\n");
            if (!_proto.TryIndex(salary.Key, out var indexedDep))
                continue;

            var name = Loc.GetString(indexedDep.Name);
            sb.Append(Loc.GetString("cp14-salary-entry",
                ("dep", name),
                ("total", _currency.GetCurrencyPrettyString(salary.Value))));
        }
        return sb.ToString();
    }

    private void OnStationPostInit(Entity<CP14StationSalaryComponent> ent, ref StationPostInitEvent args)
    {
        ent.Comp.NextSalaryTime = _timing.CurTime + ent.Comp.SalaryFrequency;
    }
}
