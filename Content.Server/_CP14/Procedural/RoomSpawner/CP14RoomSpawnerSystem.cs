using Content.Server.Procedural;
using Content.Shared.Procedural;
using Content.Shared.Random.Helpers;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.Procedural.RoomSpawner;

public sealed class CP14RoomSpawnerSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly DungeonSystem _dungeon = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedMapSystem _maps = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14RoomSpawnerComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<CP14RoomSpawnerComponent> spawner, ref MapInitEvent args)
    {
        SpawnRoom(spawner);
        QueueDel(spawner);
    }

    private void SpawnRoom(Entity<CP14RoomSpawnerComponent> spawner)
    {
        if (!_proto.TryIndex(spawner.Comp.RoomsRandom, out var rooms))
            return;

        var roomProto = rooms.Pick(_random);

        if (!_proto.TryIndex<DungeonRoomPrototype>(roomProto, out var room))
        {
            Log.Error($"Unable to find matching room prototype ({room}) for {ToPrettyString(spawner)}");
            return;
        }

        var gridUid = Transform(spawner).GridUid;

        if (!TryComp<MapGridComponent>(gridUid, out var gridComp))
            return;

        var xform = Transform(spawner).Coordinates.Offset(-room.Size / 2);
        var random = new Random();

        _dungeon.SpawnRoom(
            gridUid.Value,
            gridComp,
            _maps.LocalToTile(gridUid.Value, gridComp, xform),
            room,
            random,
            null,
            spawner.Comp.ClearExisting,
            spawner.Comp.Rotation,
            ignoreTiles: spawner.Comp.IgnoreTiles);
    }
}
