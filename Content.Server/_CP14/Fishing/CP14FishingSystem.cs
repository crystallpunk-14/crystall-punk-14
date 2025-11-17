using System.Linq;
using System.Numerics;
using Content.Shared._CP14.Fishing;
using Content.Shared._CP14.Fishing.Components;
using Content.Shared.EntityTable;
using Robust.Server.GameObjects;
using Robust.Server.GameStates;
using Robust.Server.Player;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Fishing;

public sealed class CP14FishingSystem : CP14SharedFishingSystem
{
    [Dependency] private readonly EntityTableSystem _entityTable = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly PvsOverrideSystem _pvs= default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private MapId? _mapId;

    private EntityQuery<CP14FishingPondComponent> _pondQuery;
    private EntityQuery<CP14FishComponent> _fishQuery;

    public override void Initialize()
    {
        base.Initialize();

        _pondQuery = GetEntityQuery<CP14FishingPondComponent>();
        _fishQuery = GetEntityQuery<CP14FishComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        base.Update(frameTime);

        var curTime = _gameTiming.CurTime;
        var query = EntityQueryEnumerator<CP14FishingRodComponent>();

        // Seeding prediction doesnt work
        while (query.MoveNext(out var uid, out var fishRod))
        {
            TryToCatchFish((uid, fishRod), curTime);
        }
    }

    /// <summary>
    /// Tries to cath fish
    /// </summary>
    private bool TryToCatchFish(Entity<CP14FishingRodComponent> rod, TimeSpan curTime)
    {
        if (rod.Comp.CaughtFish is not null)
            return false;

        if (rod.Comp.User is null)
            return false;

        if (rod.Comp.FishingFloat is null)
            return false;

        if (rod.Comp.Target is null)
            return false;

        if (curTime < rod.Comp.FishingTime)
            return false;

        var pond = rod.Comp.Target;
        if (!_pondQuery.TryComp(pond, out var pondComp))
            return false;

        if (pondComp.LootTable is null)
            return false;

        if (!_proto.Resolve(pondComp.LootTable, out var lootTable))
            return false;

        var fishes = _entityTable.GetSpawns(lootTable, _random.GetRandom());
        var fishId = fishes.First();

        EnsurePausedMap();
        var fish = PredictedSpawnAtPosition(fishId, new EntityCoordinates(_map.GetMap(_mapId!.Value), Vector2.Zero));

        if (!_player.TryGetSessionByEntity(rod.Comp.User.Value, out var session))
            return false;

        if (!_fishQuery.TryComp(fish, out var fishComp))
            return false;

        _pvs.AddSessionOverride(fish, session);

        rod.Comp.CaughtFish = fish;
        fishComp.GetAwayTime = curTime;
        fishComp.GetAwayTime += TimeSpan.FromSeconds(_random.NextDouble(rod.Comp.MinAwaitTime, rod.Comp.MaxAwaitTime));
        DirtyField(rod, rod.Comp, nameof(CP14FishingRodComponent.CaughtFish));
        DirtyField(fish, fishComp, nameof(CP14FishComponent.GetAwayTime));

        return true;
    }

    /// <summary>
    /// Ensures that paused map exists
    /// </summary>
    private void EnsurePausedMap()
    {
        if (_map.MapExists(_mapId))
            return;

        var mapUid = _map.CreateMap(out var newMapId);
        _meta.SetEntityName(mapUid, Loc.GetString("fishing-paused-map-name"));
        _mapId = newMapId;
        _map.SetPaused(mapUid, true);
    }
}
