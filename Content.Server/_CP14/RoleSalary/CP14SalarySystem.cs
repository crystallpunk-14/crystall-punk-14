using Content.Server._CP14.Cargo;
using Content.Server.Station.Events;
using Content.Shared._CP14.Cargo;
using Content.Shared.Paper;
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

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14StationSalaryComponent, StationPostInitEvent>(OnStationPostInit);
        SubscribeLocalEvent<CP14SalarySpawnerComponent, MapInitEvent>(OnSalaryInit);
    }

    private void OnSalaryInit(Entity<CP14SalarySpawnerComponent> ent, ref MapInitEvent args)
    {
        //Hardcode warning! ^-^

        var xform = Transform(ent);

        var paper = Spawn("CP14Paper"); //TODO Special named paper
        _transform.PlaceNextTo(paper, (ent, xform));
        if (TryComp<PaperComponent>(paper, out var paperComp))
        {
            paperComp.Content = GenerateSalaryText();
            _paper.TryStamp((paper, paperComp),
                new StampDisplayInfo
                {
                    StampedColor = Color.Red,
                    StampedName = Loc.GetString("cp14-stamp-salary"),
                },
                "red_on_paper");
        }
    }

    private string GenerateSalaryText()
    {
        return "lox";
    }

    private void OnStationPostInit(Entity<CP14StationSalaryComponent> ent, ref StationPostInitEvent args)
    {
        ent.Comp.NextSalaryTime = _timing.CurTime + ent.Comp.SalaryFrequency;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14StationSalaryComponent, CP14StationTravelingStoreShipTargetComponent>();
        while (query.MoveNext(out var uid, out var salary, out var store))
        {
            if (_timing.CurTime < salary.NextSalaryTime)
                continue;

            salary.NextSalaryTime += salary.SalaryFrequency;
            _cargo.AddBuyQueue((uid, store), new List<EntProtoId> {salary.SalaryProto});
        }
    }
}
