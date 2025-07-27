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
            return;

        foreach (var dun in dungeons)
        {
            foreach (var tile in dun.AllTiles)
            {
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
