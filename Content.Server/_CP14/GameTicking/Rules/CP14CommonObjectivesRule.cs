using Content.Server._CP14.GameTicking.Rules.Components;
using Content.Server._CP14.StationCommonObjectives;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Shared.GameTicking.Components;
using Content.Shared.Objectives.Components;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.GameTicking.Rules;

public sealed class CP14CommonObjectivesRule : GameRuleSystem<CP14CommonObjectivesRuleComponent>
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawning);
    }

    protected override void Started(EntityUid uid,
        CP14CommonObjectivesRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var query = EntityQueryEnumerator<CP14StationCommonObjectivesComponent>();
        while (query.MoveNext(out var stationUid, out var stationObj))
        {
            foreach (var jobObj in component.JobObjectives)
            {
                foreach (var weightGroupProto in jobObj.Value)
                {
                    if (!_proto.TryIndex(weightGroupProto, out var weightGroup))
                        continue;

                    var objectiveProto = weightGroup.Pick(_random);

                    if (!TryCreateCommonObjective(objectiveProto, out var objective) || objective is null)
                        continue;

                    stationObj.JobObjectives.Add(objective.Value, jobObj.Key);
                }
            }

            foreach (var depObj in component.DepartmentObjectives)
            {
                foreach (var weightGroupProto in depObj.Value)
                {
                    if (!_proto.TryIndex(weightGroupProto, out var weightGroup))
                        continue;

                    var objectiveProto = weightGroup.Pick(_random);

                    if (!TryCreateCommonObjective(objectiveProto, out var objective) || objective is null)
                        continue;

                    stationObj.DepartmentObjectives.Add(objective.Value, depObj.Key);
                }
            }
        }
    }

    private bool TryCreateCommonObjective(string objectiveProto, out EntityUid? objective)
    {
        objective = null;

        if (!_proto.HasIndex<EntityPrototype>(objectiveProto))
        {
            Log.Error($"Invalid objective prototype {objectiveProto}, don't found entity prototype");
            return false;
        }

        objective = Spawn(objectiveProto);

        if (!TryComp<ObjectiveComponent>(objective, out var comp)) //TODO: мы не можем в ObjectiveSystem делать цели без привязки к разуму. Поэтому щиткодим создания тут.
        {
            Del(objective);
            Log.Error($"Invalid objective prototype {objectiveProto}, missing ObjectiveComponent");
            return false;
        }

        var afterEv = new ObjectiveAfterAssignEvent(null, null, comp, MetaData(objective.Value));
        RaiseLocalEvent(objective.Value, ref afterEv);

        return true;
    }

    private void OnPlayerSpawning(PlayerSpawnCompleteEvent args)
    {
        //TODO: Multiply station support required
        if (!TryComp<CP14StationCommonObjectivesComponent>(args.Station, out var stationCommonObjectives))
            return;

        if (!_mind.TryGetMind(args.Mob, out var mindId, out var mind))
            return;

        foreach (var jobObj in stationCommonObjectives.JobObjectives)
        {
            if (args.JobId is null || args.JobId != jobObj.Value.Id)
                continue;


            _mind.AddObjective(mindId, mind, jobObj.Key);
        }

        foreach (var depObj in stationCommonObjectives.DepartmentObjectives)
        {
            if (args.JobId is null)
                continue;

            if (!_proto.TryIndex(depObj.Value, out var department))
                continue;

            if (!department.Roles.Contains(args.JobId))
                continue;

            _mind.AddObjective(mindId, mind, depObj.Key);
        }
    }
}
