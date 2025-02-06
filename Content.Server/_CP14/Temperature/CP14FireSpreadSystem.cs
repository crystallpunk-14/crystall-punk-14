using System.Linq;
using System.Numerics;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Nutrition.Components;
using Content.Server.Nutrition.EntitySystems;
using Content.Shared._CP14.Temperature;
using Content.Shared.Maps;
using Content.Shared.Nutrition.Components;
using Content.Shared.Smoking;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Temperature;

public sealed partial class CP14FireSpreadSystem : CP14SharedFireSpreadSystem
{
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly TileSystem _tile = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDef = default!;
    [Dependency] private readonly SmokingSystem _smoking = default!;

    private readonly EntProtoId _fireProto = "CP14Fire";

    private readonly HashSet<Entity<CP14FireSpreadComponent>> _spreadEnts = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlammableComponent, CP14IgnitionDoAfter>(OnFlammableIgnited);
        SubscribeLocalEvent<SmokableComponent, CP14IgnitionDoAfter>(OnDelayedIgnite);
        SubscribeLocalEvent<SmokingPipeComponent, CP14IgnitionDoAfter>(OnDelayedPipeIgnite);
        SubscribeLocalEvent<CP14FlammableBonusDamageComponent, MeleeHitEvent>(OnFlammableMeleeHit);
    }

    private void OnFlammableMeleeHit(Entity<CP14FlammableBonusDamageComponent> ent, ref MeleeHitEvent args)
    {
        if (!TryComp<FlammableComponent>(ent, out var flammable))
            return;

        if (!flammable.OnFire)
            return;

        args.BonusDamage += ent.Comp.DamagePerStack * flammable.FireStacks;
    }

    private void OnFlammableIgnited(Entity<FlammableComponent> ent, ref CP14IgnitionDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        _flammable.AdjustFireStacks(ent, ent.Comp.FirestacksOnIgnite, ent.Comp, true);

        args.Handled = true;
    }

    //For smokable cigars
    private void OnDelayedIgnite(Entity<SmokableComponent> ent, ref CP14IgnitionDoAfter args)
    {
        _smoking.SetSmokableState(ent, SmokableState.Lit, ent.Comp);
    }

    //For smokable pipes
    private void OnDelayedPipeIgnite(Entity<SmokingPipeComponent> pipe, ref CP14IgnitionDoAfter args)
    {
        if (!TryComp<SmokableComponent>(pipe, out var smokable))
            return;

        if (_smoking.TryTransferReagents(pipe, (pipe.Owner, smokable)))
            _smoking.SetSmokableState(pipe, SmokableState.Lit, smokable);
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
        _spreadEnts.Clear();

        var query = EntityQueryEnumerator<CP14ActiveFireSpreadingComponent, CP14FireSpreadComponent, FlammableComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var spread, out var flammable, out var xform))
        {
            if (!flammable.OnFire)
                continue;

            if (spread.NextSpreadTime >= _gameTiming.CurTime)
                continue;

            if (xform.ParentUid != xform.GridUid) //we can't set a fire if we're inside a chest, for example.
                continue;

            var cooldown = _random.NextFloat(spread.SpreadCooldownMin, spread.SpreadCooldownMax);
            spread.NextSpreadTime = _gameTiming.CurTime + TimeSpan.FromSeconds(cooldown);

            _spreadEnts.Add(new Entity<CP14FireSpreadComponent>(uid, spread));
        }

        foreach (var uid in _spreadEnts)
        {
            IgniteEntities(uid);
            IgniteTiles(uid);
        }
    }

    private void IgniteEntities(Entity<CP14FireSpreadComponent> spread)
    {
        var targets = _lookup.GetEntitiesInRange<FlammableComponent>(_transform.GetMapCoordinates(spread),
            spread.Comp.Radius,
            LookupFlags.Uncontained);
        foreach (var target in targets)
        {
            if (!_random.Prob(spread.Comp.Prob))
                continue;

            _flammable.Ignite(target, spread);
        }
    }

    private void IgniteTiles(Entity<CP14FireSpreadComponent> spread)
    {
        var xform = Transform(spread);
        // Ignore items inside containers
        if (!TryComp<MapGridComponent>(xform.ParentUid, out var grid))
            return;

        var localPos = xform.Coordinates.Position;
        var tileRefs = _mapSystem.GetLocalTilesIntersecting(xform.ParentUid,
                grid,
                new Box2(
                    localPos + new Vector2(-spread.Comp.Radius, -spread.Comp.Radius),
                    localPos + new Vector2(spread.Comp.Radius, spread.Comp.Radius)))
            .ToList();

        foreach (var tileRef in tileRefs)
        {
            if (!_random.Prob(spread.Comp.ProbTile))
                continue;

            var tile = tileRef.Tile.GetContentTileDefinition();

            if (tile.BurnedTile is null)
                continue;

            Spawn(_fireProto, _mapSystem.ToCenterCoordinates(tileRef, grid));
            _tile.ReplaceTile(tileRef, (ContentTileDefinition)_tileDef[tile.BurnedTile]);
        }
    }
}
