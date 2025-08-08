using System.Linq;
using System.Numerics;
using Content.Server.Station.Systems;
using Content.Shared._CP14.Demiplane;
using Content.Shared._CP14.Demiplane.Components;
using Content.Shared._CP14.Procedural.Prototypes;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.Demiplane;

public sealed partial class CP14DemiplaneSystem
{
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly StationSystem _station = default!;

    private ProtoId<CP14ProceduralModifierPrototype> FirstPointProto = "DemiplaneArcSingle";
    private ProtoId<CP14ProceduralModifierPrototype> SecondPointProto = "CP14DemiplanEnterRoom";

    private void InitializeStation()
    {
        SubscribeLocalEvent<CP14StationDemiplaneMapComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CP14DemiplaneNavigationMapComponent, BeforeActivatableUIOpenEvent>(
            OnBeforeActivatableUiOpen);
    }

    private void OnMapInit(Entity<CP14StationDemiplaneMapComponent> ent, ref MapInitEvent args)
    {
        GenerateDemiplaneMap(ent);
    }

    private void OnBeforeActivatableUiOpen(Entity<CP14DemiplaneNavigationMapComponent> ent,
        ref BeforeActivatableUIOpenEvent args)
    {
        var station = _station.GetOwningStation(ent, Transform(ent));

        if (!TryComp<CP14StationDemiplaneMapComponent>(station, out var stationMap))
            return;

        UpdateNodesStatus((station.Value, stationMap));

        _userInterface.SetUiState(ent.Owner,
            CP14DemiplaneMapUiKey.Key,
            new CP14DemiplaneMapUiState(stationMap.Nodes, stationMap.Edges));
    }

    private void UpdateNodesStatus(Entity<CP14StationDemiplaneMapComponent> ent)
    {
        //foreach (var node in ent.Comp.Nodes)
        //{
        //    node.Value.InFrontierZone = NodeInFronrierZone(ent.Comp.Nodes, ent.Comp.Edges, node.Key);
        //    node.Value.InUsing = false;
        //}
//
        //var query = EntityQueryEnumerator<CP14DemiplaneMapNodeBlockerComponent>();
        //while (query.MoveNext(out var uid, out var blocker))
        //{
        //    if (!TryComp<CP14StationDemiplaneMapComponent>(blocker.Station, out var stationMap))
        //        continue;
//
        //    if (!stationMap.Nodes.TryGetValue(blocker.Position, out var node))
        //        continue;
//
        //    node.InUsing = true;
        //}
    }

    private void GenerateDemiplaneMap(Entity<CP14StationDemiplaneMapComponent> ent)
    {
        ent.Comp.Nodes.Clear();
        ent.Comp.Edges.Clear();

        var directions = new[] { new Vector2i(1, 0), new Vector2i(-1, 0), new Vector2i(0, 1), new Vector2i(0, -1) };

        // Generate village node
        var villageNode = new CP14DemiplaneMapNode(Vector2.Zero, null, null)
        {
            Start = true,
        };

        ent.Comp.Nodes.Add(Vector2i.Zero, villageNode);

        // Generate first node
        var firstNodePosition = _random.Pick(directions);
        var location = SelectLocation(0);
        var modifiers = SelectModifiers(0, location, GetLimits(0));
        var firstNode = new CP14DemiplaneMapNode(firstNodePosition, location, modifiers)
        {
            Opened = true
        };

        ent.Comp.Nodes.Add(firstNodePosition, firstNode);
        ent.Comp.Edges.Add((Vector2i.Zero, firstNodePosition));

        //In a loop, take a random existing node, find an empty spot next to it on the grid, add a new room there, and connect them.
        while (ent.Comp.Nodes.Count < ent.Comp.TotalCount)
        {
            //Get random existing node
            var randomNode = _random.Pick(ent.Comp.Nodes);

            if (randomNode.Value.Start)
                continue;

            var randomNodePosition = randomNode.Key;

            // Find a random empty adjacent position
            var emptyPositions = directions
                .Select(dir => randomNodePosition + dir)
                .Where(pos => !ent.Comp.Nodes.ContainsKey(pos))
                .ToList();

            if (emptyPositions.Count == 0)
                continue;

            var newPosition = emptyPositions[_random.Next(emptyPositions.Count)];

            // Add the new node and connect it with an edge
            var lvl = Math.Abs(newPosition.X) + Math.Abs(newPosition.Y);
            location = SelectLocation(lvl);
            modifiers = SelectModifiers(lvl, location, GetLimits(lvl));
            var newNode = new CP14DemiplaneMapNode(newPosition, location, modifiers);

            ent.Comp.Nodes[newPosition] = newNode;
            ent.Comp.Edges.Add((randomNodePosition, newPosition));

            randomNode.Value.Modifiers.Add(FirstPointProto);
            newNode.Modifiers.Add(SecondPointProto);
        }

        foreach (var (position, node) in ent.Comp.Nodes)
        {
            //Random minor UI offset
            node.UiPosition += new Vector2(
                _random.NextFloat(-0.2f, 0.2f),
                _random.NextFloat(-0.2f, 0.2f));
        }
    }

    private Dictionary<ProtoId<CP14ProceduralModifierCategoryPrototype>, float> GetLimits(int level)
    {
        return new Dictionary<ProtoId<CP14ProceduralModifierCategoryPrototype>, float>
        {
            { "Danger", Math.Max(level * 0.2f, 0.5f) },
            { "GhostRoleDanger", 1f },
            { "Reward", Math.Max(level * 0.3f, 0.5f) },
            { "Ore", Math.Max(level * 0.5f, 1f) },
            { "Fun", 1f },
            { "Weather", 1f },
            { "MapLight", 1f },
            { "Passage", 1f },
        };
    }
}
