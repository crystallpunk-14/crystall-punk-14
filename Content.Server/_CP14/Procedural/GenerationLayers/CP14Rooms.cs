using System.Threading.Tasks;
using Content.Shared.Maps;
using Content.Shared.Procedural;
using Content.Shared.Procedural.DungeonLayers;
using Robust.Shared.Random;

namespace Content.Server.Procedural.DungeonJob;

public sealed partial class DungeonJob
{
    /// <summary>
    /// <see cref="OreDunGen"/>
    /// </summary>
    private async Task PostGen(
        CP14RoomsDunGen gen,
        List<Dungeon> dungeons,
        HashSet<Vector2i> reservedTiles,
        Random random)
    {
        HashSet<Vector2i> processedTiles = new();
        List<DungeonRoomPrototype> availableRooms = new();
        var availableTiles = new List<Vector2i>();

        foreach (var room in _prototype.EnumeratePrototypes<DungeonRoomPrototype>())
        {
            foreach (var tag in room.Tags)
            {
                if (gen.Tags.Contains(tag))
                {
                    availableRooms.Add(room);
                    break;
                }
            }
        }

        if (availableRooms.Count == 0)
        {
            _sawmill.Warning($"No rooms found for generation with tags: {string.Join(", ", gen.Tags)}");
        }

        foreach (var dun in dungeons)
        {
            foreach (var tile in dun.AllTiles)
            {
                if (processedTiles.Contains(tile))
                    continue;

                processedTiles.Add(tile);

                var tileRef = _maps.GetTileRef(_gridUid, _grid, tile);

                if (reservedTiles.Contains(tile))
                    continue;

                //Tile mask filtering
                if (gen.TileMask is not null)
                {
                    if (!gen.TileMask.Contains(((ContentTileDefinition)_tileDefManager[tileRef.Tile.TypeId]).ID))
                        continue;
                }
                else
                {
                    //If entity mask null - we ignore the tiles that have anything on them.
                    if (!_anchorable.TileFree((_gridUid, _grid),
                            tile,
                            DungeonSystem.CollisionLayer,
                            DungeonSystem.CollisionMask))
                        continue;
                }

                // Add it to valid nodes.
                availableTiles.Add(tile);

                await SuspendDungeon();

                if (!ValidateResume())
                    return;
            }
        }

        var roomCount = gen.Count;
        while (roomCount > 0 && availableTiles.Count > 0)
        {
            await SuspendDungeon();

            if (!ValidateResume())
                return;

            var selectedTile = random.PickAndTake(availableTiles);
            availableTiles.Remove(selectedTile);

            var room = random.Pick(availableRooms);

            var roomBounds = new List<Vector2i>();
            var conflict = false;

            for (int x = 0; x < room.Size.X; x++)
            {
                for (int y = 0; y < room.Size.Y; y++)
                {
                    var pos = selectedTile + new Vector2i(x, y) - new Vector2i(room.Size.X/2,room.Size.Y/2);
                    if (reservedTiles.Contains(pos))
                    {
                        conflict = true;
                        break;
                    }
                    roomBounds.Add(pos);
                }
                if (conflict) break;
            }

            if (conflict)
                continue;

            _dungeon.SpawnRoom(_gridUid, _grid, selectedTile - new Vector2i(room.Size.X/2,room.Size.Y/2), room, random, clearExisting: true, rotation: true);

            foreach (var pos in roomBounds)
            {
                reservedTiles.Add(pos);
            }

            roomCount--;
        }

        if (roomCount > 0)
        {
            _sawmill.Warning($"Not enough space to generate all rooms. Wanted: {gen.Count}, Generated: {gen.Count - roomCount}");
        }
    }
}
