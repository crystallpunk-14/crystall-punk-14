using System.Linq;
using System.Threading;
using Content.Server._CP14.Demiplane.Components;
using Content.Server._CP14.Demiplane.Jobs;
using Content.Server._CP14.RoundEnd;
using Content.Server.GameTicking;
using Content.Shared._CP14.Demiplane.Components;
using Content.Shared._CP14.Demiplane.Prototypes;
using Content.Shared._CP14.MagicManacostModify;
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
        SubscribeLocalEvent<CP14DemiplaneGeneratorDataComponent, MapInitEvent>(GeneratorMapInit);
        SubscribeLocalEvent<CP14DemiplaneGeneratorDataComponent, UseInHandEvent>(GeneratorUsedInHand);

        SubscribeLocalEvent<CP14DemiplaneGeneratorDataComponent, GetVerbsEvent<ExamineVerb>>(OnVerbExamine);
    }

    private void OnVerbExamine(Entity<CP14DemiplaneGeneratorDataComponent> ent, ref GetVerbsEvent<ExamineVerb> args)
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

    private FormattedMessage GetDemiplanExamine(CP14DemiplaneGeneratorDataComponent comp)
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
    public void SpawnRandomDemiplane(ProtoId<CP14DemiplaneLocationPrototype> location, List<ProtoId<CP14DemiplaneModifierPrototype>> modifiers, out Entity<CP14DemiplaneComponent> demiplan, out MapId mapId)
    {
        var mapUid = _mapSystem.CreateMap(out mapId, runMapInit: false);
        var demiComp = EntityManager.EnsureComponent<CP14DemiplaneComponent>(mapUid);
        demiplan = (mapUid, demiComp);

        var cancelToken = new CancellationTokenSource();
        var job = new CP14SpawnRandomDemiplaneJob(
            JobMaxTime,
            EntityManager,
            _logManager,
            _mapManager,
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

    private void GeneratorUsedInHand(Entity<CP14DemiplaneGeneratorDataComponent> generator, ref UseInHandEvent args)
    {
        if (generator.Comp.Location is null)
            return;

        if (_gameTicker.RunLevel != GameRunLevel.InRound)
        {
            _popup.PopupEntity(Loc.GetString("cp14-demiplan-cannot-open-end-round"), generator, args.User);
            return;
        }
        //We cant open demiplan in another demiplan or if parent is not Map
        if (HasComp<CP14DemiplaneComponent>(Transform(generator).MapUid) || !HasComp<MapGridComponent>(_transform.GetParentUid(args.User)))
        {
            _popup.PopupEntity(Loc.GetString("cp14-demiplan-cannot-open", ("name", MetaData(generator).EntityName)), generator, args.User);
            return;
        }

        SpawnRandomDemiplane(generator.Comp.Location.Value, generator.Comp.SelectedModifiers, out var demiplane, out var mapId);

        //Admin log needed
        EnsureComp<CP14DemiplaneDestroyWithoutStabilizationComponent>(demiplane);

        //Ура, щиткод и магические переменные!
        var tempRift = EntityManager.Spawn("CP14DemiplaneTimedRadiusPassway");
        var tempRift2 = EntityManager.Spawn("CP14DemiplanRiftCore");
        _transform.SetCoordinates(tempRift, Transform(args.User).Coordinates);
        _transform.SetCoordinates(tempRift2, Transform(args.User).Coordinates);

        var connection = EnsureComp<CP14DemiplaneRiftComponent>(tempRift);
        var connection2 = EnsureComp<CP14DemiplaneRiftComponent>(tempRift2);
        AddDemiplanRandomExitPoint(demiplane, (tempRift, connection));
        AddDemiplanRandomExitPoint(demiplane, (tempRift2, connection2));

#if !DEBUG
        QueueDel(generator); //wtf its crash debug build!
#endif
    }

    private void GeneratorMapInit(Entity<CP14DemiplaneGeneratorDataComponent> generator, ref MapInitEvent args)
    {
        //Location generation
        HashSet<CP14DemiplaneLocationPrototype> suitableConfigs = new();
        foreach (var locationConfig in _proto.EnumeratePrototypes<CP14DemiplaneLocationPrototype>())
        {
            suitableConfigs.Add(locationConfig);
        }

        CP14DemiplaneLocationPrototype? selectedConfig = null;
        while (suitableConfigs.Count > 0)
        {
            var randomConfig = _random.Pick(suitableConfigs);

            if (!generator.Comp.TiersContent.ContainsKey(randomConfig.Tier))
            {
                suitableConfigs.Remove(randomConfig);
                continue;
            }

            if (!_random.Prob(generator.Comp.TiersContent[randomConfig.Tier]))
            {
                suitableConfigs.Remove(randomConfig);
                continue;
            }

            selectedConfig = randomConfig;
            break;
        }

        if (selectedConfig is null)
        {
            // We dont should be here

            Log.Warning("Expedition mission generation failed: No suitable location configs.");
            QueueDel(generator);
            return;
        }

        generator.Comp.Location = selectedConfig;

        //Modifier generation
        Dictionary<CP14DemiplaneModifierPrototype, float> suitableModifiersWeights = new();
        foreach (var modifier in _proto.EnumeratePrototypes<CP14DemiplaneModifierPrototype>())
        {
            var passed = true;
            //Tag blacklist filter
            foreach (var configTag in selectedConfig.Tags)
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
                    if (!selectedConfig.Tags.Contains(reqTag))
                    {
                        passed = false;
                        break;
                    }
                }
            }

            //Tier filter
            if (passed)
            {
                var innerPassed = false;
                foreach (var tier in modifier.Tiers)
                {
                    if (generator.Comp.TiersContent.ContainsKey(tier))
                    {
                        innerPassed = true;
                        break;
                    }
                }

                if (!innerPassed)
                {
                    passed = false;
                }
            }

            // Tier weight filter
            if (passed)
            {
                var maxProb = 0f;
                foreach (var tier in modifier.Tiers)
                {
                    if (generator.Comp.TiersContent.ContainsKey(tier))
                        maxProb = Math.Max(maxProb, generator.Comp.TiersContent[tier]);
                }

                if (!_random.Prob(maxProb))
                {
                    passed = false;
                }
            }

            //Random prob filter
            if (passed)
            {
                if (!_random.Prob(modifier.GenerationProb))
                {
                    passed = false;
                }
            }

            if (passed)
                suitableModifiersWeights.Add(modifier, modifier.GenerationWeight);
        }


        //Limits calculation
        Dictionary<ProtoId<CP14DemiplaneModifierCategoryPrototype>, float> limits = new();
        foreach (var limit in generator.Comp.Limits)
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

            generator.Comp.SelectedModifiers.Add(selectedModifier);

            foreach (var category in selectedModifier.Categories)
            {
                limits[category.Key] -= category.Value;
            }

            if (selectedModifier.Unique)
                suitableModifiersWeights.Remove(selectedModifier);
        }

        //Scenario generation

        //ETC generation
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
