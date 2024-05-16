using Content.Shared.Maps;
using Content.Shared.Teleportation.Components;
using Content.Shared.Teleportation.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;

namespace Content.Server._CP14.DungeonPortal;

public sealed partial class CP14AutoDungeonPortalSystem : EntitySystem
{
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly LinkedEntitySystem _linkedEntity = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefManager = default!;
    [Dependency] private readonly TileSystem _tile = default!;
    [Dependency] private readonly SharedMapSystem _maps = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14AutoDungeonPortalComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<CP14AutoDungeonPortalComponent> autoPortal, ref MapInitEvent args)
    {
        if (!TryComp<PortalComponent>(autoPortal, out var portalComp))
            return;

        var mapId = Transform(autoPortal).MapID;
        _map.TryGetMap(mapId, out var mapUid);

        if (mapUid == null)
            return;

        if (!TryComp<LinkedEntityComponent>(mapUid, out var link))
            return;

        if (link.LinkedEntities.Count > 1) //Bruh, we don't want more than 1 linked maps for now
            return;

        var targetMapUid = _random.Pick(link.LinkedEntities);
        var targetMapId = Transform(targetMapUid).MapID;

        var currentWorldPos = _transform.GetWorldPosition(autoPortal);
        var targetMapPos = new MapCoordinates(currentWorldPos, targetMapId);

        var otherSidePortal = Spawn(autoPortal.Comp.OtherSidePortal, targetMapPos);

        if (_linkedEntity.TryLink(autoPortal, otherSidePortal, true))
            RemComp<CP14AutoDungeonPortalComponent>(autoPortal);

        ClearOtherSide(otherSidePortal, targetMapUid);
    }

    private void ClearOtherSide(EntityUid otherSidePortal, EntityUid targetMapUid)
    {
        var tiles = new List<(Vector2i Index, Tile Tile)>();
        var originF = _transform.GetWorldPosition(otherSidePortal);
        var origin = new Vector2i((int) originF.X, (int) originF.Y);
        var tileDef = _tileDefManager["CP14FloorStonebricks"]; //TODO: Remove hardcode
        var seed = _random.Next();
        var random = new Random(seed);
        var grid = Comp<MapGridComponent>(targetMapUid);

        for (var x = -2; x <= 2; x++) //TODO: Remove hardcode
        {
            for (var y = -2; y <= 2; y++)
            {
                tiles.Add((new Vector2i(x, y) + origin, new Tile(tileDef.TileId, variant: _tile.PickVariant((ContentTileDefinition) tileDef, random))));
            }
        }

        _maps.SetTiles(targetMapUid, grid, tiles);
    }
}
