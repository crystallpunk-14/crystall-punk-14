using System.Threading;
using Content.Server._CP14.Demiplane.Components;
using Content.Server._CP14.Demiplane.Jobs;
using Content.Shared._CP14.Demiplane.Components;
using Content.Shared._CP14.Demiplane.Prototypes;
using Content.Shared.Interaction.Events;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.Demiplane;

public sealed partial class CP14DemiplaneSystem
{
    private readonly JobQueue _expeditionQueue = new();
    private readonly List<(CP14SpawnRandomDemiplaneJob Job, CancellationTokenSource CancelToken)> _expeditionJobs = new();
    private const double JobMaxTime = 0.002;

    private void InitGeneration()
    {
        SubscribeLocalEvent<CP14DemiplaneGeneratorDataComponent, MapInitEvent>(GeneratorMapInit);
        SubscribeLocalEvent<CP14DemiplaneGeneratorDataComponent, UseInHandEvent>(GeneratorUsedInHand);
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

        //We cant open demiplan in another demiplan
        if (HasComp<CP14DemiplaneComponent>(Transform(generator).MapUid))
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

        QueueDel(generator); //wtf its crash debug build?
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
        HashSet<CP14DemiplaneModifierPrototype> suitableModifiers = new();
        foreach (var modifier in _proto.EnumeratePrototypes<CP14DemiplaneModifierPrototype>())
        {
            //Tag filtering here
            suitableModifiers.Add(modifier);
        }

        var selectedModifier = _random.Pick(suitableModifiers);

        generator.Comp.Modifiers.Add(selectedModifier);

        //Scenario generation

        //ETC generation
    }
}
