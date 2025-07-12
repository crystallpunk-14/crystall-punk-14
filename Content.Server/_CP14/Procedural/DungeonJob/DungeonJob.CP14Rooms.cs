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
        // Doesn't use dungeon data because layers and we don't need top-down support at the moment.

        List<DungeonRoomPrototype> availableRooms = new();

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
            return;

        var availableTiles = new List<Vector2i>();
        var tiles = _maps.GetAllTilesEnumerator(_gridUid, _grid);

        // WARNING:
        // This DunGen handles not only the tiles of the passed dungeon, but ALL the tiles of the current grid
        // So don't run it anywhere
        while (tiles.MoveNext(out var tileRef))
        {
            var tile = tileRef.Value.GridIndices;

            //Tile mask filtering
            if (gen.TileMask is not null)
            {
                if (!gen.TileMask.Contains(((ContentTileDefinition)_tileDefManager[tileRef.Value.Tile.TypeId]).ID))
                    continue;
            }
            else
            {
                //If entity mask null - we ignore the tiles that have anything on them.
                if (!_anchorable.TileFree(_grid, tile, DungeonSystem.CollisionLayer, DungeonSystem.CollisionMask))
                    continue;
            }

            // Add it to valid nodes.
            availableTiles.Add(tile);

            await SuspendDungeon();

            if (!ValidateResume())
                return;
        }

        for (var i = 0; i < gen.Count; i++)
        {
            await SuspendDungeon();

            if (!ValidateResume())
                return;

            var selectedTile = random.PickAndTake(availableTiles);

            availableTiles.Remove(selectedTile);

            var room = random.Pick(availableRooms);
            _dungeon.SpawnRoom(_gridUid, _grid, selectedTile, room , random);
        }
    }
}
