using System.Linq;
using System.Threading;
using Content.Server._CP14.Demiplane.Components;
using Content.Server._CP14.Demiplane.Jobs;
using Content.Server.GameTicking;
using Content.Shared._CP14.Demiplane.Components;
using Content.Shared._CP14.Demiplane.Prototypes;
using Content.Shared.Examine;
using Content.Shared.Interaction.Events;
using Content.Shared.Verbs;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Robust.Shared.Map.Components;


namespace Content.Server._CP14.Demiplane;

public sealed partial class CP14DemiplaneSystem
{
    private readonly JobQueue _expeditionQueue = new();
    private readonly List<(CP14SpawnRandomDemiplaneJob Job, CancellationTokenSource CancelToken)> _expeditionJobs = new();
    private const double JobMaxTime = 0.002;

    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    private void InitGeneration()
    {
        SubscribeLocalEvent<CP14DemiplaneRandomGeneratorComponent, MapInitEvent>(GeneratorMapInit);
        SubscribeLocalEvent<CP14DemiplaneUsingOpenComponent, UseInHandEvent>(GeneratorUsedInHand);

        SubscribeLocalEvent<CP14DemiplaneDataComponent, GetVerbsEvent<ExamineVerb>>(OnVerbExamine);
    }

    private void GeneratorMapInit(Entity<CP14DemiplaneRandomGeneratorComponent> generator, ref MapInitEvent args)
    {
        if (!TryComp<CP14DemiplaneDataComponent>(generator, out var data))
            return;

        CP14DemiplaneLocationPrototype? selectedConfig = null;
        //Location generation
        if (data.Location is null || generator.Comp.OverrideLocation)
        {
            selectedConfig = GenerateDemiplaneLocation(generator.Comp.Level);
            data.Location = selectedConfig;
        }
        else
        {
            if (!_proto.TryIndex(data.Location, out selectedConfig))
                return;
        }

        //Modifier generation
        var newModifiers = GenerateDemiplaneModifiers(
            generator.Comp.Level,
            selectedConfig,
            generator.Comp.Limits);

        foreach (var mod in newModifiers)
        {
            data.SelectedModifiers.Add(mod);
        }
    }

    private void GeneratorUsedInHand(Entity<CP14DemiplaneUsingOpenComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<CP14DemiplaneDataComponent>(ent, out var generator))
            return;

        args.Handled = true;

        UseGenerator((ent, generator), args.User);

        QueueDel(ent);
    }

    //Ed: I hate this function.
    private void UseGenerator(Entity<CP14DemiplaneDataComponent> generator, EntityUid? user = null)
    {
        //block the opening of demiplanes after the end of a round
        if (_gameTicker.RunLevel != GameRunLevel.InRound)
        {
            if (user is not null)
                _popup.PopupEntity(Loc.GetString("cp14-demiplan-cannot-open-end-round"), generator, user.Value);
            return;
        }
        //We cant open demiplane in another demiplane or if parent is not Map
        if (_demiplaneQuery.HasComp(Transform(generator).MapUid))
        {
            if (user is not null)
                _popup.PopupEntity(Loc.GetString("cp14-demiplan-cannot-open", ("name", MetaData(generator).EntityName)), generator, user.Value);
            return;
        }

        if (generator.Comp.Location is null)
            return;

        //an attempt to open demiplanes can be intercepted by other systems that substitute a map instead of generating the planned demiplane.
        Entity<CP14DemiplaneComponent>? demiplane = null;
        var ev = new CP14DemiplaneGenerationCatchAttemptEvent();
        RaiseLocalEvent(ev);

        if (ev.Demiplane is null)
        {
            SpawnRandomDemiplane(generator.Comp.Location.Value, generator.Comp.SelectedModifiers, out demiplane, out var mapId);
            if (demiplane is not null && TryComp<CP14DemiplaneMapNodeBlockerComponent>(generator, out var blocker))
            {
                EnsureComp<CP14DemiplaneMapNodeBlockerComponent>(demiplane.Value, out var blockerMap);
                blockerMap.Position = blocker.Position;
                blockerMap.Station = blocker.Station;
                blockerMap.IncreaseNodeDifficulty = 1;
            }
        }
        else
        {
            demiplane = ev.Demiplane;
        }

        if (demiplane is null)
            return;

        //Admin log needed
        EnsureComp<CP14DemiplaneDestroyWithoutStabilizationComponent>(demiplane.Value);

        //Rifts spawning
        foreach (var rift in generator.Comp.AutoRifts)
        {
            var spawnedRift = EntityManager.Spawn(rift);
            _transform.SetCoordinates(spawnedRift, Transform(generator).Coordinates);
            _transform.AttachToGridOrMap(spawnedRift);
            var connection = EnsureComp<CP14DemiplaneRiftComponent>(spawnedRift);
            AddDemiplaneRandomExitPoint(demiplane.Value, (spawnedRift, connection));
        }
    }

    private void OnVerbExamine(Entity<CP14DemiplaneDataComponent> ent, ref GetVerbsEvent<ExamineVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        var markup = GetDemiplanExamine(ent.Comp);
        _examine.AddDetailedExamineVerb(
            args,
            ent.Comp,
            markup,
            Loc.GetString("cp14-demiplan-examine"),
            "/Textures/Interface/VerbIcons/dot.svg.192dpi.png"); //TODO custom icon
    }

    private FormattedMessage GetDemiplanExamine(CP14DemiplaneDataComponent comp)
    {
        var msg = new FormattedMessage();

        if (!_proto.TryIndex(comp.Location, out var indexedLocation))
            return msg;

        msg.AddMarkupOrThrow(
            indexedLocation.Name is not null/* && _random.Prob(indexedLocation.ExamineProb)*/
                ? Loc.GetString("cp14-demiplane-examine-title", ("location", Loc.GetString(indexedLocation.Name)))
                : Loc.GetString("cp14-demiplane-examine-title-unknown"));

        List<LocId> modifierNames = new();
        foreach (var modifier in comp.SelectedModifiers)
        {
            if (!_proto.TryIndex(modifier, out var indexedModifier))
                continue;

            //if (!_random.Prob(indexedModifier.ExamineProb)) //temp disable
            //    continue;

            if (indexedModifier.Name is null)
                continue;

            if (modifierNames.Contains(indexedModifier.Name.Value))
                continue;

            modifierNames.Add(indexedModifier.Name.Value);
        }

        if (modifierNames.Count > 0)
        {
            msg.AddMarkupOrThrow("\n" + Loc.GetString("cp14-demiplane-examine-modifiers"));
            foreach (var name in modifierNames)
            {
                msg.AddMarkupOrThrow("\n- " + Loc.GetString(name));
            }
        }

        return msg;
    }

    private void UpdateGeneration(float frameTime)
    {
        _expeditionQueue.Process();

        foreach (var (job, cancelToken) in _expeditionJobs.ToArray())
        {
            switch (job.Status)
            {
                case JobStatus.Finished:
                    _expeditionJobs.Remove((job, cancelToken));
                    break;
            }
        }
    }

    /// <summary>
    /// Generates a new random demiplane based on the specified parameters
    /// </summary>
    public void SpawnRandomDemiplane(ProtoId<CP14DemiplaneLocationPrototype> location, List<ProtoId<CP14DemiplaneModifierPrototype>> modifiers, out Entity<CP14DemiplaneComponent>? demiplane, out MapId mapId)
    {
        var mapUid = _mapSystem.CreateMap(out mapId, runMapInit: false);
        var demiComp = EntityManager.EnsureComponent<CP14DemiplaneComponent>(mapUid);
        demiplane = (mapUid, demiComp);

        var cancelToken = new CancellationTokenSource();
        var job = new CP14SpawnRandomDemiplaneJob(
            JobMaxTime,
            EntityManager,
            _logManager,
            _proto,
            _dungeon,
            _metaData,
            _mapSystem,
            mapUid,
            mapId,
            location,
            modifiers,
            _random.Next(-10000, 10000),
            cancelToken.Token);

        _expeditionJobs.Add((job, cancelToken));
        _expeditionQueue.EnqueueJob(job);
    }


    /// <summary>
    /// Returns a suitable demiplane location for the specified difficulty level.
    /// </summary>
    public CP14DemiplaneLocationPrototype GenerateDemiplaneLocation(int level)
    {
        CP14DemiplaneLocationPrototype? selectedConfig = null;

        HashSet<CP14DemiplaneLocationPrototype> suitableConfigs = new();
        foreach (var locationConfig in _proto.EnumeratePrototypes<CP14DemiplaneLocationPrototype>())
        {
            suitableConfigs.Add(locationConfig);
        }

        while (suitableConfigs.Count > 0)
        {
            var randomConfig = _random.Pick(suitableConfigs);

            //LevelRange filter
            if (level < randomConfig.Levels.Min || level > randomConfig.Levels.Max)
            {
                suitableConfigs.Remove(randomConfig);
                continue;
            }

            selectedConfig = randomConfig;
            break;
        }

        if (selectedConfig is null)
            throw new Exception($"No suitable demiplane location config found for level {level}!");

        return selectedConfig;
    }


    /// <summary>
    /// Returns a set of modifiers under the specified difficulty level that are appropriate for the specified demiplane location
    /// </summary>
    public List<CP14DemiplaneModifierPrototype> GenerateDemiplaneModifiers(
        int level,
        CP14DemiplaneLocationPrototype location,
        Dictionary<ProtoId<CP14DemiplaneModifierCategoryPrototype>,float> modifierLimits)
    {
        List<CP14DemiplaneModifierPrototype> selectedModifiers = new();

        //Modifier generation
        Dictionary<CP14DemiplaneModifierPrototype, float> suitableModifiersWeights = new();
        foreach (var modifier in _proto.EnumeratePrototypes<CP14DemiplaneModifierPrototype>())
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
        Dictionary<ProtoId<CP14DemiplaneModifierCategoryPrototype>, float> limits = new();
        foreach (var limit in modifierLimits)
        {
            limits.Add(limit.Key, limit.Value);
        }


        while (suitableModifiersWeights.Count > 0)
        {
            var selectedModifier = ModifierPick(suitableModifiersWeights, _random);

            //Fill demiplane under limits
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
    private static CP14DemiplaneModifierPrototype ModifierPick(Dictionary<CP14DemiplaneModifierPrototype, float> weights, IRobustRandom random)
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

public sealed class CP14DemiplaneGenerationCatchAttemptEvent : EntityEventArgs
{
    public bool Handled = false;
    public Entity<CP14DemiplaneComponent>? Demiplane;
}
