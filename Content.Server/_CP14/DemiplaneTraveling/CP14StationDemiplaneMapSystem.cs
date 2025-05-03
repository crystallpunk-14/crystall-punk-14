using System.Linq;
using System.Numerics;
using Content.Server._CP14.Demiplane;
using Content.Server._CP14.Demiplane.Components;
using Content.Server.Station.Systems;
using Content.Shared._CP14.Demiplane.Prototypes;
using Content.Shared._CP14.DemiplaneTraveling;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.DemiplaneTraveling;

public sealed partial class CP14StationDemiplaneMapSystem : CP14SharedStationDemiplaneMapSystem
{
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly CP14DemiplaneSystem _demiplane = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14StationDemiplaneMapComponent, ComponentInit>(OnMapInit);

        SubscribeLocalEvent<CP14DemiplaneMapComponent, CP14DemiplaneMapEjectMessage>(DemiplaneEjectAttempt);
        SubscribeLocalEvent<CP14DemiplaneMapComponent, BeforeActivatableUIOpenEvent>(OnBeforeActivatableUiOpen);
    }

    private void DemiplaneEjectAttempt(Entity<CP14DemiplaneMapComponent> ent, ref CP14DemiplaneMapEjectMessage args)
    {
        var station = _station.GetOwningStation(ent, Transform(ent));

        if (!TryComp<CP14StationDemiplaneMapComponent>(station, out var stationMap))
            return;

        if (!stationMap.Nodes.TryGetValue(args.Position, out var node))
            return;

        if (!node.Ejectable)
            return;

        //Eject!
        var key = SpawnAttachedTo("CP14BaseDemiplaneKey", Transform(ent).Coordinates);
        if (TryComp<CP14DemiplaneDataComponent>(key, out var demiData))
        {
            demiData.Location = node.LocationConfig;
            demiData.SelectedModifiers = node.Modifiers;
        }
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
        GenerateDemiplaneMap(ent);
        UpdateNodeStatus(ent);
    }

    public void UpdateUIStates()
    {
        var query = EntityQueryEnumerator<CP14DemiplaneMapComponent>();
        while (query.MoveNext(out var uid, out var demiplaneMap))
        {
            var station = _station.GetOwningStation(uid, Transform(uid));

            if (!TryComp<CP14StationDemiplaneMapComponent>(station, out var stationMap))
                continue;

            _userInterface.SetUiState(uid, CP14DemiplaneMapUiKey.Key, new CP14DemiplaneMapUiState(stationMap.Nodes, stationMap.Edges));
        }
    }

    private void GenerateDemiplaneMap(Entity<CP14StationDemiplaneMapComponent> ent)
    {
        ent.Comp.Nodes.Clear();
        ent.Comp.Edges.Clear();

        var allSpecials = _proto.EnumeratePrototypes<CP14SpecialDemiplanePrototype>().ToList();
        _random.Shuffle(allSpecials);

        var grid = new Dictionary<Vector2i, CP14DemiplaneMapNode>();

        //Spawn start room at 0 0
        var startPos = new Vector2i(0, 0);
        var startNode = new CP14DemiplaneMapNode(0, startPos, true);
        grid[startPos] = startNode;

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

            var specialPos = new Vector2i(0, 0);
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

            var specialNode = new CP14DemiplaneMapNode(
                specialLevel,
                new Vector2(specialPos.X, specialPos.Y),
                false,
                locationConfig: special.Location,
                modifiers: special.Modifiers
            );
            grid[specialPos] = specialNode;
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

                if (delta.X != 0)
                    options.Add(new Vector2i(Math.Sign(delta.X), 0));
                if (delta.Y != 0)
                    options.Add(new Vector2i(0, Math.Sign(delta.Y)));

                // Add the possibility of a "mistaken" step to the side
                if (_random.Prob(0.3f)) // 30% chance to take a side step
                {
                    if (delta.X != 0 && delta.Y != 0)
                    {
                        options.Add(new Vector2i(0, Math.Sign(delta.Y)) * -1);
                        options.Add(new Vector2i(Math.Sign(delta.X), 0) * -1);
                    }
                }

                _random.Shuffle(options);
                var step = options[0];
                var next = current + step;

                if (!grid.TryGetValue(next, out var nextNode))
                {
                    nextNode = new CP14DemiplaneMapNode(Math.Abs(next.X) + Math.Abs(next.Y), new Vector2(next.X, next.Y), false);
                    grid[next] = nextNode;
                }

                ent.Comp.Edges.Add((current, next));

                current = next;
            }
        }

        //Fill nodes with random data
        foreach (var node in grid.Values)
        {
            if (node.Level == 0)
                continue;

            var location = _demiplane.GenerateDemiplaneLocation(node.Level);
            node.LocationConfig ??= location;

            var limits = new Dictionary<ProtoId<CP14DemiplaneModifierCategoryPrototype>, float>
            {
                { "Danger", node.Level * 0.2f },
                { "GhostRoleDanger", node.Level * 0.2f },
                { "Reward", Math.Max(node.Level * 0.2f, 0.5f) },
                { "Fun", 1f },
                { "Weather", 1f },
                { "MapLight", 1f },
            };
            var mods = _demiplane.GenerateDemiplaneModifiers(node.Level, location, limits);
            foreach (var mod in mods)
            {
                node.Modifiers.Add(mod);
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
        ent.Comp.Nodes = grid;
    }

    private void UpdateNodeStatus(Entity<CP14StationDemiplaneMapComponent> ent)
    {
        foreach (var node in ent.Comp.Nodes)
        {
            node.Value.Ejectable = CanEjectCoordinates(ent.Comp.Nodes, ent.Comp.Edges, node.Key);
        }

        UpdateUIStates();
    }
}
