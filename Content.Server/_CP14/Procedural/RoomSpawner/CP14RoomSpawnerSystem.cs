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
        if (!_random.Prob(spawner.Comp.Prob))
            return;

        var rooms = new HashSet<DungeonRoomPrototype>();

        foreach (var roomProto in _proto.EnumeratePrototypes<DungeonRoomPrototype>())
        {
            var whitelisted = false;

            foreach (var tag in spawner.Comp.RoomsTag)
            {
                if (roomProto.Tags.Contains(tag))
                {
                    whitelisted = true;
                    break;
                }
            }

            if (!whitelisted)
                continue;

            rooms.Add(roomProto);
        }

        if (rooms.Count == 0)
            return;

        var selectedRoom = _random.Pick(rooms);

        if (!_proto.TryIndex<DungeonRoomPrototype>(selectedRoom, out var room))
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
            spawner.Comp.Rotation);
    }
}
