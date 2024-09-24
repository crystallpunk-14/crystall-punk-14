using System.Linq;
using System.Numerics;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Maps;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Temperature;

public sealed partial class CP14FireSpreadSystem : EntitySystem
{
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly TileSystem _tile = default!;
    [Dependency] private readonly ITileDefinitionManager _tiledef = default!;

    private EntProtoId _fireProto = "CP14Fire";
    public override void Initialize()
    {
        SubscribeLocalEvent<CP14FireSpreadComponent, OnFireChangedEvent>(OnCompInit);
        SubscribeLocalEvent<CP14DespawnOnExtinguishComponent, OnFireChangedEvent>(OnFireChanged);
    }

    private void OnFireChanged(Entity<CP14DespawnOnExtinguishComponent> ent, ref OnFireChangedEvent args)
    {
        if (!args.OnFire)
            QueueDel(ent);
    }

    private void OnCompInit(Entity<CP14FireSpreadComponent> ent, ref OnFireChangedEvent args)
    {
        if (!args.OnFire)
            return;

        var cooldown = _random.NextFloat(ent.Comp.SpreadCooldownMin, ent.Comp.SpreadCooldownMax);
        ent.Comp.NextSpreadTime = _gameTiming.CurTime + TimeSpan.FromSeconds(cooldown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateFireSpread();
        UpdateAutoIgnite();
    }

    private void UpdateAutoIgnite()
    {
        var query = EntityQueryEnumerator<CP14AutoIgniteComponent, FlammableComponent>();
        while (query.MoveNext(out var uid, out var autoIgnite, out var flammable))
        {
            if (!autoIgnite.Initialized || !flammable.Initialized)
                continue;

            if (autoIgnite.IgniteTime == TimeSpan.Zero)
                autoIgnite.IgniteTime = _gameTiming.CurTime + autoIgnite.IgniteDelay;

            if (_gameTiming.CurTime < autoIgnite.IgniteTime)
                continue;
            //Это такой пиздец, что-то сломалось у оффов, что поджигание сущности в момент инициализации (например MapInitEvent) нахрен все крашит.
            //Поэтому я добавил 1-секундную задержку перед поджиганием.

            _flammable.AdjustFireStacks(uid, autoIgnite.StartStack, flammable, true);
            RemCompDeferred<CP14AutoIgniteComponent>(uid);
        }
    }

    private void UpdateFireSpread()
    {
        List<Entity<CP14FireSpreadComponent>> spreadUids = new();
        var query = EntityQueryEnumerator<CP14FireSpreadComponent, FlammableComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var spread, out var flammable, out var xform))
        {
            if (!flammable.OnFire)
                continue;

            if (spread.NextSpreadTime >= _gameTiming.CurTime)
                continue;

            if (xform.ParentUid != xform.GridUid) //we can't set a fire if we're inside a chest, for example.
                continue;

            var cooldown = _random.NextFloat(spread.SpreadCooldownMin, spread.SpreadCooldownMax);
            spread.NextSpreadTime = _gameTiming.CurTime + TimeSpan.FromSeconds(cooldown);

            spreadUids.Add(new Entity<CP14FireSpreadComponent>(uid, spread));
        }

        foreach (var uid in spreadUids)
        {
            IgniteEntities(uid, uid.Comp);
            IgniteTiles(uid, uid.Comp);
        }
    }

    private void IgniteEntities(EntityUid uid, CP14FireSpreadComponent spread)
    {
        var targets = _lookup.GetEntitiesInRange<FlammableComponent>(_transform.GetMapCoordinates(uid), spread.Radius, LookupFlags.Uncontained);
        foreach (var target in targets)
        {
            if (!_random.Prob(spread.Prob))
                continue;

            _flammable.Ignite(target, uid);
        }
    }

    private void IgniteTiles(EntityUid uid, CP14FireSpreadComponent spread)
    {
        var xform = Transform(uid);
        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return;

        // Ignore items inside containers
        if (!HasComp<MapGridComponent>(xform.ParentUid))
            return;

        var localPos = xform.Coordinates.Position;
        var tileRefs = _mapSystem.GetLocalTilesIntersecting(grid.Owner,
                grid,
                new Box2(
                    localPos + new Vector2(-spread.Radius, -spread.Radius),
                    localPos + new Vector2(spread.Radius, spread.Radius)))
            .ToList();


        foreach (var tileref in tileRefs)
        {
            if (!_random.Prob(spread.ProbTile))
                continue;

            var tile = tileref.Tile.GetContentTileDefinition();

            if (tile.BurnedTile is null)
                continue;

            Spawn(_fireProto, _mapSystem.ToCenterCoordinates(tileref, grid));
            _tile.ReplaceTile(tileref, (ContentTileDefinition) _tiledef[tile.BurnedTile]);
        }
    }
}
