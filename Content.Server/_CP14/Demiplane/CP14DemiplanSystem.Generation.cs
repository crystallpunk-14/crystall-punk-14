using System.Linq;
using System.Threading;
using Content.Server._CP14.Demiplane.Components;
using Content.Server._CP14.Demiplane.Jobs;
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
        foreach (var modifier in comp.Modifiers)
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

        //We cant open demiplan in another demiplan or if parent is not Map
        if (HasComp<CP14DemiplaneComponent>(Transform(generator).MapUid) || !HasComp<MapGridComponent>(_transform.GetParentUid(args.User)))
        {
            _popup.PopupEntity(Loc.GetString("cp14-demiplan-cannot-open", ("name", MetaData(generator).EntityName)), generator, args.User);
            return;
        }

        SpawnRandomDemiplane(generator.Comp.Location.Value, generator.Comp.Modifiers, out var demiplane, out var mapId);

        //Admin log needed
        //TEST
        EnsureComp<CP14DemiplaneDestroyWithoutStabilizationComponent>(demiplane);

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

        if (suitableConfigs.Count == 0)
        {
            Log.Error("Expedition mission generation failed: No suitable location configs.");
            QueueDel(generator);
            return;
        }

        var selectedConfig = _random.Pick(suitableConfigs);
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
            foreach (var reqTag in modifier.RequiredTags)
            {
                if (!selectedConfig.Tags.Contains(reqTag))
                {
                    passed = false;
                    break;
                }
            }

            if (passed)
            {
                suitableModifiersWeights.Add(modifier, modifier.GenerationWeight);
            }
        }

        var difficulty = 0f;
        var reward = 0f;
        while (generator.Comp.Modifiers.Count < generator.Comp.MaxModifiers && suitableModifiersWeights.Count > 0)
        {
            var selectedModifier = ModifierPick(suitableModifiersWeights, _random);
            if (difficulty + selectedModifier.Difficulty > generator.Comp.DifficultyLimit)
            {
                suitableModifiersWeights.Remove(selectedModifier);
                continue;
            }

            if (reward + selectedModifier.Reward > generator.Comp.RewardLimit)
            {
                suitableModifiersWeights.Remove(selectedModifier);
                continue;
            }

            generator.Comp.Modifiers.Add(selectedModifier);
            reward += selectedModifier.Reward;
            difficulty += selectedModifier.Difficulty;

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
