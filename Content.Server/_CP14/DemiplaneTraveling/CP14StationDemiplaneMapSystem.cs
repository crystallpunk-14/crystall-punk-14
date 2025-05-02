using System.Linq;
using System.Numerics;
using Content.Server.Station.Systems;
using Content.Shared._CP14.DemiplaneTraveling;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Random;

namespace Content.Server._CP14.DemiplaneTraveling;

public sealed partial class CP14SharedDemiplaneMapSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14StationDemiplaneMapComponent, ComponentInit>(OnMapInit);
        SubscribeLocalEvent<CP14DemiplaneMapComponent, BeforeActivatableUIOpenEvent>(OnBeforeActivatableUiOpen);
    }

    private void OnBeforeActivatableUiOpen(Entity<CP14DemiplaneMapComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        var station = _station.GetOwningStation(ent, Transform(ent));

        if (!TryComp<CP14StationDemiplaneMapComponent>(station, out var stationMap))
            return;

        _userInterface.SetUiState(ent.Owner, CP14DemiplaneMapUiKey.Key, new CP14DemiplaneMapUiState(stationMap.Nodes));
    }

    private void OnMapInit(Entity<CP14StationDemiplaneMapComponent> ent, ref ComponentInit args)
    {
        GenerateDemiplaneMap(ent);
    }

    private void GenerateDemiplaneMap(Entity<CP14StationDemiplaneMapComponent> ent)
    {
        ent.Comp.Nodes.Clear();

        int gridSize = 9;
        var center = new Vector2i(4, 4);

        var directions = new List<Vector2i>
        {
            new (0, -1), // top
            new (0, 1),  // down
            new (-1, 0), // left
            new (1, 0),  // right
        };

        var grid = new Dictionary<Vector2i, CP14DemiplaneMapNode>();
        var used = new HashSet<Vector2i>();

        // 1. Выбрать 5 целевых точек на границах
        List<Vector2i> targetRooms = new();
        while (targetRooms.Count < 5)
        {
            var x = _random.Next(0, gridSize);
            var y = _random.Next(0, gridSize);
            if ((x == 0 || x == 9 || y == 0 || y == 9) && new Vector2i(x, y) != center)
            {
                var p = new Vector2i(x, y);
                if (!targetRooms.Contains(p))
                    targetRooms.Add(p);
            }
        }

        // 2. Создать центр
        var centerKey = $"node_{center.X}_{center.Y}";
        var centerNode = new CP14DemiplaneMapNode(centerKey, new Vector2(center.X, center.Y), true, "T1Caves", [ "RoyalPumpkin" ]);
        grid[center] = centerNode;
        used.Add(center);

        // 3. Прокладываем путь до каждой цели
        foreach (var target in targetRooms)
        {
            var current = center;
            while (current != target)
            {
                var candidates = directions
                    .Select(d => new Vector2i(current.X + d.X, current.Y + d.Y))
                    .Where(p => p.X >= 0 && p.X < gridSize && p.Y >= 0 && p.Y < gridSize)
                    .ToList();

                Vector2i next;

                if (_random.Prob(0.3f)) // 30% шанс сделать случайный шаг
                {
                    next = candidates
                        .Where(p => p != current)
                        .OrderBy(_ => _random.Next())
                        .First();
                }
                else
                {
                    next = candidates
                        .OrderBy(p => Distance(p, target) + (used.Contains(p) ? 5 : 0))
                        .First();
                }

                // создать комнату если нужно
                if (!grid.ContainsKey(next))
                {
                    string key = $"node_{next.X}_{next.Y}";
                    var pos = new Vector2(next.X, next.Y);
                    var location = _random.Pick(new[] { "T1GrasslandIsland", "T1Caves","T1MagmaCaves", "T1IceCaves", "T1SnowIsland" });
                    var node = new CP14DemiplaneMapNode(key, pos, false, location, [ "RoyalPumpkin" ]);
                    grid[next] = node;
                    used.Add(next);
                }

                // создать связь от current к next
                grid[current].Childrens.Add($"node_{next.X}_{next.Y}");
                current = next;
            }
        }

        // 4. Добавить случайный сдвиг координат в пределах 0.5
        foreach (var node in grid.Values)
        {
            var x = node.UiPosition.X + _random.NextFloat(-0.2f, 0.2f);
            var y = node.UiPosition.Y + _random.NextFloat(-0.2f, 0.2f);
            node.UiPosition = new Vector2(x, y);
        }

        // 5. Случайные связи между соседями
        //var existingEdges = new HashSet<(string, string)>();
//
        //foreach (var kvp in grid)
        //{
        //    var pos = kvp.Key;
        //    var node = kvp.Value;
        //    var fromKey = $"node_{pos.X}_{pos.Y}";
//
        //    foreach (var dir in directions)
        //    {
        //        var neighborPos = pos + dir;
        //        if (!grid.TryGetValue(neighborPos, out var neighbor))
        //            continue;
//
        //        var toKey = $"node_{neighborPos.X}_{neighborPos.Y}";
//
        //        // проверка, есть ли уже связь в любом направлении
        //        if (existingEdges.Contains((fromKey, toKey)) || existingEdges.Contains((toKey, fromKey)))
        //            continue;
//
        //        if (_random.Prob(0.10f))
        //        {
        //            node.Childrens.Add(toKey);
        //            existingEdges.Add((fromKey, toKey));
        //        }
        //    }
        //}

        // 6. Добавить все узлы в сущность
        foreach (var node in grid.Values)
        {
            ent.Comp.Nodes.Add(node);
        }
    }

    // Евклидово расстояние (можно заменить Manhattan)
    private float Distance(Vector2i a, Vector2i b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }
}
