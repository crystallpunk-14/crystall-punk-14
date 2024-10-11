using Content.Server.Shuttles.Systems;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
using Content.Shared._CP14.Currency;
using Content.Shared._CP14.TravelingStoreShip;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.TravelingStoreShip;

public sealed partial class CP14CargoSystem : CP14SharedCargoSystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MapLoaderSystem _loader = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ShuttleSystem _shuttles = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly CP14CurrencySystem _currency = default!;

    private EntityQuery<TransformComponent> _xformQuery;

    public override void Initialize()
    {
        base.Initialize();

        _xformQuery = GetEntityQuery<TransformComponent>();

        InitializeStore();
        InitializeShuttle();

        SubscribeLocalEvent<CP14StationTravelingStoreshipTargetComponent, StationPostInitEvent>(OnPostInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateShuttle();
    }

    private void OnPostInit(Entity<CP14StationTravelingStoreshipTargetComponent> station, ref StationPostInitEvent args)
    {
        if (!Deleted(station.Comp.Shuttle))
            return;

        var tradepostMap = _mapManager.CreateMap();

        if (!_loader.TryLoad(tradepostMap, station.Comp.ShuttlePath.ToString(), out var shuttleUids))
            return;

        var shuttle =  shuttleUids[0];
        station.Comp.Shuttle = shuttle;
        station.Comp.TradepostMap = _mapManager.GetMapEntityId(tradepostMap);
        var travelingStoreShipComp = EnsureComp<CP14TravelingStoreShipComponent>(station.Comp.Shuttle);
        travelingStoreShipComp.Station = station;

        SendShuttleToStation(station, 5); //Start fast travel
        UpdateStorePositions(station);
    }

    private void UpdateStorePositions(Entity<CP14StationTravelingStoreshipTargetComponent> station)
    {
        station.Comp.CurrentBuyPositions.Clear();
        station.Comp.CurrentSellPositions.Clear();

        //Static add
        foreach (var position in station.Comp.StaticBuyPositions)
        {
            if (!_proto.TryIndex(position, out var indexedP))
                continue;
            station.Comp.CurrentBuyPositions.Add(position, indexedP.Price.Next(_random));
        }
        foreach (var position in station.Comp.StaticSellPositions)
        {
            if (!_proto.TryIndex(position, out var indexedP))
                continue;
            station.Comp.CurrentSellPositions.Add(position, indexedP.Price.Next(_random));
        }
        //Dynamic add

    }

    private void SellingThings(Entity<CP14StationTravelingStoreshipTargetComponent> station)
    {
        var shuttle = station.Comp.Shuttle;

        //Get all sended to tradepost entities
        var toSell = new HashSet<EntityUid>();

        var query = EntityQueryEnumerator<CP14SellingPalettComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var selling, out var palletXform))
        {
            if (palletXform.ParentUid != shuttle || !palletXform.Anchored)
                continue;

            var seldEnt = new HashSet<EntityUid>();

            _lookup.GetEntitiesIntersecting(uid, seldEnt, LookupFlags.Dynamic | LookupFlags.Sundries);

            foreach (var ent in seldEnt)
            {
                if (toSell.Contains(ent) || !_xformQuery.TryGetComponent(ent, out var xform))
                    continue;

                toSell.Add(ent);
            }
        }

        foreach (var sellPos in station.Comp.CurrentSellPositions)
        {
            if (!_proto.TryIndex(sellPos.Key, out var indexedPos))
                continue;

            while (indexedPos.Service.TrySell(EntityManager, toSell))
            {
                station.Comp.Balance += sellPos.Value;
            }
        }
    }
}
