using System.Linq;
using Content.Server._CP14.Demiplane.Components;
using Content.Server._CP14.Procedural;
using Content.Shared._CP14.Demiplane;
using Content.Shared._CP14.Demiplane.Components;
using Content.Shared._CP14.Procedural.Prototypes;
using Content.Shared.Interaction;
using Content.Shared.Teleportation.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Demiplane;

public sealed partial class CP14DemiplaneSystem : CP14SharedDemiplaneSystem
{
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly CP14LocationGenerationSystem _generation = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly LinkedEntitySystem _link = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ILogManager _logManager = default!;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeStation();

        _sawmill = _logManager.GetSawmill("demiplane_map_gen");

        SubscribeLocalEvent<CP14DemiplaneRiftComponent, InteractHandEvent>(OnRiftInteracted);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14DemiplaneRiftComponent>();
        while (query.MoveNext(out var uid, out var demiplaneRift))
        {
            if (demiplaneRift.ScanningTargetMap is null)
                continue;

            if (_timing.CurTime < demiplaneRift.NextScanTime)
                continue;

            demiplaneRift.NextScanTime = _timing.CurTime + TimeSpan.FromSeconds(5);

            var targetQuery = EntityQueryEnumerator<CP14DemiplaneEnterPointComponent>();
            while (targetQuery.MoveNext(out var enterUid, out var enterComp))
            {
                if (Transform(enterUid).MapUid != demiplaneRift.ScanningTargetMap)
                    continue;

                //Remove awaiting
                QueueDel(demiplaneRift.AwaitingEntity);

                //Start connection
                var portal1 = SpawnAtPosition(demiplaneRift.PortalProto, Transform(enterUid).Coordinates);
                var portal2 = SpawnAtPosition(demiplaneRift.PortalProto, Transform(uid).Coordinates);
                _link.TryLink(portal1, portal2, true);

                //Delete self
                QueueDel(uid);
                QueueDel(enterUid);

                return;
            }
        }
    }

    private void OnRiftInteracted(Entity<CP14DemiplaneRiftComponent> ent, ref InteractHandEvent args)
    {
        if (HasComp<CP14DemiplaneBlockInteractionsComponent>(args.User))
            return;

        if (!ent.Comp.CanCreate)
            return;

        var station = _station.GetStations().First();
        if (!TryComp<CP14StationDemiplaneMapComponent>(station, out var stationMap))
        {
            _sawmill.Error($"Station {station} does not have a CP14StationDemiplaneMapComponent!");
            QueueDel(ent);
            return;
        }

        var currentPosition = Vector2i.Zero;
        var originMap = Transform(ent).MapUid;

        if (TryComp<CP14DemiplaneMapComponent>(originMap, out var originDemiplaneComp))
            currentPosition = originDemiplaneComp.Position;

        var targetPosition = GetRandomNeighbourNotGeneratedMap((station, stationMap), currentPosition);

        if (targetPosition is null || !stationMap.Nodes.TryGetValue(targetPosition.Value, out var nextMapNode))
        {
            _sawmill.Warning($"No suitable target position found for rift at {currentPosition} in station {station}. " +
                             $"Either all neighbours are generated or the map is not initialized properly.");
            QueueDel(ent);
            return;
        }

        if (nextMapNode.LocationConfig is null)
        {
            _sawmill.Error($"Next map node at {targetPosition} does not have a location config! " +
                           $"Cannot generate demiplane map.");

            QueueDel(ent);
            return;
        }

        stationMap.GeneratedNodes.Add(targetPosition.Value);
        _map.CreateMap(out var mapId, runMapInit: false);
        var mapUid = _map.GetMap(mapId);
        EnsureComp<CP14DemiplaneMapComponent>(mapUid).Position = targetPosition.Value;

        _meta.SetEntityName(mapUid, $"Demi: [{targetPosition}] - {nextMapNode.LocationConfig}");

        _generation.GenerateLocation(
            mapUid,
            mapId,
            nextMapNode.LocationConfig.Value,
            nextMapNode.Modifiers);

        var awaiting = SpawnAtPosition(ent.Comp.AwaitingProto, Transform(ent).Coordinates);

        ent.Comp.AwaitingEntity = awaiting;
        ent.Comp.ScanningTargetMap = mapUid;
        ent.Comp.CanCreate = false;
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
    public List<ProtoId<CP14ProceduralModifierPrototype>> SelectModifiers(
        int level,
        CP14ProceduralLocationPrototype location,
        Dictionary<ProtoId<CP14ProceduralModifierCategoryPrototype>, float> modifierLimits)
    {
        List<ProtoId<CP14ProceduralModifierPrototype>> selectedModifiers = new();

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

public sealed class CP14LocationGeneratedEvent : EntityEventArgs
{
}
