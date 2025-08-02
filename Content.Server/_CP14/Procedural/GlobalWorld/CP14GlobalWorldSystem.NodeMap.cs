using System.Diagnostics;
using System.Linq;
using Content.Server._CP14.Procedural.GlobalWorld.Components;
using Content.Server.Station.Components;
using Content.Shared._CP14.Procedural.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.Procedural.GlobalWorld;

public sealed partial class CP14GlobalWorldSystem
{
    private void GenerateGlobalWorldMap(Entity<CP14StationGlobalWorldComponent> ent)
    {
        ent.Comp.Nodes.Clear();
        ent.Comp.Edges.Clear();

        //For first - check station integration, and put station into (0,0) global map position
        if (TryComp<CP14StationGlobalWorldIntegrationComponent>(ent, out var integration) &&
            TryComp<StationDataComponent>(ent, out var stationData))
        {
            var largestStationGrid = _station.GetLargestGrid(stationData);

            Debug.Assert(largestStationGrid is not null);

            var mapId = _transform.GetMapId(largestStationGrid.Value);
            var zeroNode =
                new CP14GlobalWorldNode
                {
                    MapUid = mapId,
                    LocationConfig = integration.Location,
                    Modifiers = integration.Modifiers,
                    Level = 0,
                };
            GenerateNodeData(zeroNode);
            ent.Comp.Nodes.Add(
                Vector2i.Zero,
                zeroNode
            );
        }
        else
        {
            var zeroNode = new CP14GlobalWorldNode();
            GenerateNodeData(zeroNode);
            ent.Comp.Nodes.Add(Vector2i.Zero, zeroNode);
        }

        //Generate nodes with random data until limits
        while (ent.Comp.Nodes.Count < ent.Comp.LocationCount + 1)
        {
            // Get a random existing node
            var randomNode = _random.Pick(ent.Comp.Nodes);
            var randomNodePosition = randomNode.Key;

            // Find a random empty adjacent position
            var directions = new[] { new Vector2i(1, 0), new Vector2i(-1, 0), new Vector2i(0, 1), new Vector2i(0, -1) };
            var emptyPositions = directions
                .Select(dir => randomNodePosition + dir)
                .Where(pos => !ent.Comp.Nodes.ContainsKey(pos))
                .ToList();

            if (emptyPositions.Count == 0)
                continue;

            var newPosition = emptyPositions[Random.Shared.Next(emptyPositions.Count)];

            // Add the new node and connect it with an edge
            var newNode = new CP14GlobalWorldNode
            {
                Level = Math.Abs(newPosition.X) + Math.Abs(newPosition.Y),
            };
            GenerateNodeData(newNode);
            ent.Comp.Nodes.Add(newPosition, newNode);
            ent.Comp.Edges.Add((randomNodePosition, newPosition));

            //Add connection modifiers to each other
            if (_proto.TryIndex(newNode.LocationConfig, out var indexedNewNodeLocation) && _proto.TryIndex(randomNode.Value.LocationConfig, out var indexedRandomNodeLocation))
            {
                newNode.Modifiers.Add(indexedRandomNodeLocation.Connection);
                randomNode.Value.Modifiers.Add(indexedNewNodeLocation.Connection);
            }
        }
    }

    private void GenerateNodeData(CP14GlobalWorldNode node,
        bool overrideLocation = false,
        bool clearOldModifiers = false)
    {
        if (node.LocationConfig is null || overrideLocation)
        {
            var location = SelectLocation(node.Level);
            node.LocationConfig ??= location;
        }

        if (!_proto.TryIndex(node.LocationConfig, out var indexedLocation))
            throw new Exception($"No location config found for node at level {node.Level}!");

        var limits = new Dictionary<ProtoId<CP14ProceduralModifierCategoryPrototype>, float>
        {
            { "Danger", node.Level * 0.2f },
            { "GhostRoleDanger", 1f },
            { "Reward", Math.Max(node.Level * 0.2f, 0.5f) },
            { "Ore", Math.Max(node.Level * 0.2f, 0.5f) },
            { "Fun", 1f },
            { "Weather", 1f },
            { "MapLight", 1f },
        };
        var mods = SelectModifiers(node.Level, indexedLocation, limits);

        if (clearOldModifiers)
            node.Modifiers.Clear();
        foreach (var mod in mods)
        {
            node.Modifiers.Add(mod);
        }
    }

    /// <summary>
    /// Returns a suitable location for the specified difficulty level.
    /// </summary>
    public CP14ProceduralLocationPrototype SelectLocation(int level)
    {
        CP14ProceduralLocationPrototype? selectedConfig = null;

        HashSet<CP14ProceduralLocationPrototype> suitableConfigs = new();
        foreach (var locationConfig in _proto.EnumeratePrototypes<CP14ProceduralLocationPrototype>())
        {
            suitableConfigs.Add(locationConfig);
        }

        while (suitableConfigs.Count > 0)
        {
            var randomConfig = _random.Pick(suitableConfigs);

            var passed = true;

            //Random prob filter
            if (passed)
            {
                if (!_random.Prob(randomConfig.GenerationProb))
                {
                    passed = false;
                }
            }

            //Levels filter
            if (passed)
            {
                if (level < randomConfig.Levels.Min || level > randomConfig.Levels.Max)
                {
                    passed = false;
                }
            }

            if (!passed)
            {
                suitableConfigs.Remove(randomConfig);
                continue;
            }

            selectedConfig = randomConfig;
            break;
        }

        if (selectedConfig is null)
            throw new Exception($"No suitable procedural location config found for level {level}!");

        return selectedConfig;
    }

    /// <summary>
    /// Returns a set of modifiers under the specified difficulty level that are appropriate for the specified location
    /// </summary>
    public List<CP14ProceduralModifierPrototype> SelectModifiers(
        int level,
        CP14ProceduralLocationPrototype location,
        Dictionary<ProtoId<CP14ProceduralModifierCategoryPrototype>, float> modifierLimits)
    {
        List<CP14ProceduralModifierPrototype> selectedModifiers = new();

        //Modifier generation
        Dictionary<CP14ProceduralModifierPrototype, float> suitableModifiersWeights = new();
        foreach (var modifier in _proto.EnumeratePrototypes<CP14ProceduralModifierPrototype>())
        {
            var passed = true;

            //Random prob filter
            if (passed)
            {
                if (!_random.Prob(modifier.GenerationProb))
                {
                    passed = false;
                }
            }

            //Levels filter
            if (passed)
            {
                if (level < modifier.Levels.Min || level > modifier.Levels.Max)
                {
                    passed = false;
                }
            }

            //Tag blacklist filter
            foreach (var configTag in location.Tags)
            {
                if (modifier.BlacklistTags.Count != 0 && modifier.BlacklistTags.Contains(configTag))
                {
                    passed = false;
                    break;
                }
            }

            //Tag required filter
            if (passed)
            {
                foreach (var reqTag in modifier.RequiredTags)
                {
                    if (!location.Tags.Contains(reqTag))
                    {
                        passed = false;
                        break;
                    }
                }
            }

            if (passed)
                suitableModifiersWeights.Add(modifier, modifier.GenerationWeight);
        }


        //Limits calculation
        Dictionary<ProtoId<CP14ProceduralModifierCategoryPrototype>, float> limits = new();
        foreach (var limit in modifierLimits)
        {
            limits.Add(limit.Key, limit.Value);
        }


        while (suitableModifiersWeights.Count > 0)
        {
            var selectedModifier = ModifierPick(suitableModifiersWeights, _random);

            //Fill location under limits
            var passed = true;
            foreach (var category in selectedModifier.Categories)
            {
                if (!limits.ContainsKey(category.Key))
                {
                    suitableModifiersWeights.Remove(selectedModifier);
                    passed = false;
                    break;
                }

                if (limits[category.Key] - category.Value < 0)
                {
                    suitableModifiersWeights.Remove(selectedModifier);
                    passed = false;
                    break;
                }
            }

            if (!passed)
                continue;

            selectedModifiers.Add(selectedModifier);

            foreach (var category in selectedModifier.Categories)
            {
                limits[category.Key] -= category.Value;
            }

            if (selectedModifier.Unique)
                suitableModifiersWeights.Remove(selectedModifier);
        }

        return selectedModifiers;
    }

    /// <summary>
    /// Optimization moment: avoid re-indexing for weight selection
    /// </summary>
    private static CP14ProceduralModifierPrototype ModifierPick(
        Dictionary<CP14ProceduralModifierPrototype, float> weights,
        IRobustRandom random)
    {
        var picks = weights;
        var sum = picks.Values.Sum();
        var accumulated = 0f;

        var rand = random.NextFloat() * sum;

        foreach (var (key, weight) in picks)
        {
            accumulated += weight;

            if (accumulated >= rand)
            {
                return key;
            }
        }

        // Shouldn't happen
        throw new InvalidOperationException($"Invalid weighted pick in CP14DemiplanSystem.Generation!");
    }
}
