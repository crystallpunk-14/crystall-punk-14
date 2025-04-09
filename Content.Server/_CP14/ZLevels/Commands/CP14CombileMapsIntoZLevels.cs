using System.Linq;
using Content.Server._CP14.ZLevels.Components;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Map;

namespace Content.Server._CP14.ZLevels.Commands;

[AdminCommand(AdminFlags.VarEdit)]
public sealed class CP14CombineMapsIntoZLevelsCommand : LocalizedCommands
{
    [Dependency] private readonly IEntityManager _entities = default!;
    [Dependency] private readonly IMapManager _map = default!;

    private const string Name = "cp14-combineMapsIntoZLevels";
    public override string Command => Name;
    public override string Description => "Connects a number of maps into a common network of z-levels. Does not work if one of the maps is already in the z-level network";
    public override string Help => $"{Name} <MapId 1> <MapId 2> ... <MapId X> (from ground to sky)";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length == 1)
        {
            shell.WriteError("Not enough maps to form a network of levels");
            return;
        }

        List<MapId> maps = new();
        foreach (var arg in args)
        {
            if (!int.TryParse(arg, out var mapIdInt))
            {
                shell.WriteError($"Cannot parse `{arg}` into mapId");
                return;
            }

            var mapId = new MapId(mapIdInt);

            if (mapId == MapId.Nullspace)
            {
                shell.WriteError($"Cannot parse NullSpace");
                return;
            }

            if (!_map.MapExists(mapId))
            {
                shell.WriteError($"Map {mapId} dont exist");
                return;
            }

            //if (!_mapSystem.TryGetMap(mapId, out var mapUid))
            //{
            //    shell.WriteError($"Map {mapId} dont exist");
            //    return;
            //}

            if (maps.Contains(mapId))
            {
                shell.WriteError($"Duplication maps: {mapId}");
                return;
            }

            maps.Add(mapId);
        }

        //Check maps already in zLevel links
        var query = _entities.EntityQueryEnumerator<CP14StationZLevelsComponent>();
        while (query.MoveNext(out var uid, out var zLevelComp))
        {
            foreach (var findMap in maps)
            {
                if (zLevelComp.LevelEntities.ContainsKey(findMap))
                {
                    shell.WriteError($"{findMap} already in z-level network! Z-Network Entity: {uid}");
                    return;
                }
            }
        }

        //Ok, all check passed, we create new z-level network
        var zLevelEnt = _entities.Spawn();
        _entities.EnsureComponent<CP14StationZLevelsComponent>(zLevelEnt, out var newZLevelComp);

        var count = 0;
        foreach (var map in maps)
        {
            newZLevelComp.LevelEntities.Add(map, count);
            count++;
        }

        shell.WriteLine($"Successfully created z-level network! Z-Network entity: {zLevelEnt}");
    }
}
