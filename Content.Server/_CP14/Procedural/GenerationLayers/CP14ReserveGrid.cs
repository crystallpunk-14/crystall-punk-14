using System.Threading.Tasks;
using Content.Shared.Procedural;
using Content.Shared.Procedural.DungeonLayers;

namespace Content.Server.Procedural.DungeonJob;

public sealed partial class DungeonJob
{
    private async Task PostGen(CP14ReserveGrid dunGen,
        HashSet<Vector2i> reservedTiles)
    {
        var tiles = _maps.GetAllTilesEnumerator(_gridUid, _grid);
        while (tiles.MoveNext(out var tileRef))
        {
            var node = tileRef.Value.GridIndices;

            reservedTiles.Add(node);
        }
    }
}
