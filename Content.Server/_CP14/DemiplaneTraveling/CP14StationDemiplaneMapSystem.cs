using System.Linq;
using System.Numerics;
using Content.Server.Station.Systems;
using Content.Shared._CP14.Demiplane.Prototypes;
using Content.Shared._CP14.DemiplaneTraveling;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.DemiplaneTraveling;

public sealed partial class CP14SharedDemiplaneMapSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
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

        _userInterface.SetUiState(ent.Owner, CP14DemiplaneMapUiKey.Key, new CP14DemiplaneMapUiState(stationMap.Nodes, stationMap.Edges));
    }

    private void OnMapInit(Entity<CP14StationDemiplaneMapComponent> ent, ref ComponentInit args)
    {
        NewGenerateDemiplaneMap(ent);
    }

    private void NewGenerateDemiplaneMap(Entity<CP14StationDemiplaneMapComponent> ent)
    {
        ent.Comp.Nodes.Clear();
        ent.Comp.Edges.Clear();

        var allSpecials = _proto.EnumeratePrototypes<CP14SpecialDemiplanePrototype>().ToList();
        _random.Shuffle(allSpecials);

        var grid = new Dictionary<Vector2i, CP14DemiplaneMapNode>();
        var used = new HashSet<Vector2i>();
        var directions = new List<Vector2i>
        {
            new(0, -1), // top
            new(0, 1), // down
            new(-1, 0), // left
            new(1, 0), // right
        };

        //Spawn start room at 0 0
        var startPos = new Vector2i(0, 0);
        var startNode = new CP14DemiplaneMapNode("node_0_0", startPos, true);
        grid[startPos] = startNode;
        used.Add(startPos);

        //Spawn special rooms
        var specialCount = _random.Next(ent.Comp.Specials.Min, ent.Comp.Specials.Max + 1);
        var placedSpecials = 0;
        var specialPositions = new List<Vector2i>();

        foreach (var special in allSpecials)
        {
            if (placedSpecials >= specialCount)
                break;

            var specialLevel = special.Levels.Next(_random);

            var possiblePositions = new List<Vector2i>();
            for (var x = -specialLevel; x <= specialLevel; x++)
            {
                var y = specialLevel - Math.Abs(x);
                if (y != 0)
                    possiblePositions.Add(new Vector2i(x, y));
                possiblePositions.Add(new Vector2i(x, -y));
            }

            _random.Shuffle(possiblePositions);

            Vector2i specialPos = new Vector2i(0, 0);
            foreach (var pos in possiblePositions)
            {
                if (!grid.ContainsKey(pos))
                {
                    specialPos = pos;
                    break;
                }
            }

            if (grid.ContainsKey(specialPos))
                continue;

            var specialKey = $"node_{specialPos.X}_{specialPos.Y}";
            var specialNode = new CP14DemiplaneMapNode(
                specialKey,
                new Vector2(specialPos.X, specialPos.Y),
                false,
                location: special.Location,
                modifiers: special.Modifiers
            );
            grid[specialPos] = specialNode;
            used.Add(specialPos);
            specialPositions.Add(specialPos);
            placedSpecials++;
        }

        // Build meandering paths to each special room and add edges
        foreach (var specialPos in specialPositions)
        {
            var current = startPos;

            while (current != specialPos)
            {
                var delta = specialPos - current;
                var options = new List<Vector2i>();

                if (delta.X != 0) options.Add(new Vector2i(Math.Sign(delta.X), 0));
                if (delta.Y != 0) options.Add(new Vector2i(0, Math.Sign(delta.Y)));

                // Добавим возможность "ошибочного" хода в сторону
                if (_random.Prob(0.3f)) // 30% шанс сделать шаг вбок
                {
                    if (delta.X != 0 && delta.Y != 0)
                    {
                        // добавить "вбок" движение
                        options.Add(new Vector2i(0, Math.Sign(delta.Y)) * -1);
                        options.Add(new Vector2i(Math.Sign(delta.X), 0) * -1);
                    }
                }

                _random.Shuffle(options);
                var step = options[0];
                var next = current + step;

                if (!grid.TryGetValue(next, out var nextNode))
                {
                    var key = $"node_{next.X}_{next.Y}";
                    nextNode = new CP14DemiplaneMapNode(key, new Vector2(next.X, next.Y), false);
                    grid[next] = nextNode;
                    used.Add(next);
                }

                var fromKey = $"node_{current.X}_{current.Y}";
                var toKey = $"node_{next.X}_{next.Y}";
                ent.Comp.Edges.Add((fromKey, toKey));

                current = next;
            }
        }

        // Random visual offset
        foreach (var node in grid.Values)
        {
            var x = node.UiPosition.X + _random.NextFloat(-0.2f, 0.2f);
            var y = node.UiPosition.Y + _random.NextFloat(-0.2f, 0.2f);
            node.UiPosition = new Vector2(x, y);
        }

        //Add all rooms into component
        foreach (var node in grid.Values)
        {
            ent.Comp.Nodes.Add(node);
        }
    }
}
