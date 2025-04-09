using System.Text;
using Content.Server._CP14.GameTicking.Rules.Components;
using Content.Server._CP14.StationCommonObjectives;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
using Content.Shared.Random.Helpers;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.GameTicking.Rules;

public sealed class CP14CommonObjectivesRule : GameRuleSystem<CP14CommonObjectivesRuleComponent>
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;

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
            var mindComp = EnsureComp<MindComponent>(stationUid);
            component.StationMind = stationUid;

            foreach (var jobObj in component.JobObjectives)
            {
                foreach (var weightGroupProto in jobObj.Value)
                {
                    if (!_proto.TryIndex(weightGroupProto, out var weightGroup))
                        continue;

                    var objectiveProto = weightGroup.Pick(_random);

                    if (!_objectives.TryCreateObjective((component.StationMind.Value, mindComp), objectiveProto, out var objective))
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

                    if (!_objectives.TryCreateObjective((component.StationMind.Value, mindComp), objectiveProto, out var objective))
                        continue;

                    stationObj.DepartmentObjectives.Add(objective.Value, depObj.Key);
                }
            }
        }
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

    protected override void AppendRoundEndText(EntityUid uid,
        CP14CommonObjectivesRuleComponent component,
        GameRuleComponent gameRule,
        ref RoundEndTextAppendEvent args)
    {
        base.AppendRoundEndText(uid, component, gameRule, ref args);

        EnsureComp<MindComponent>(uid, out var stationMind);

        var query = EntityQueryEnumerator<CP14StationCommonObjectivesComponent>();
        while (query.MoveNext(out var objectives))
        {
            var grouped = new Dictionary<DepartmentPrototype, List<EntityUid>>();
            foreach (var department in objectives.DepartmentObjectives)
            {
                var indexedDepartment = _proto.Index(department.Value);

                if (!grouped.ContainsKey(indexedDepartment))
                    grouped.Add(indexedDepartment, new List<EntityUid>());

                grouped[indexedDepartment].Add(department.Key);
            }

            foreach (var group in grouped)
            {
                var sb = new StringBuilder();
                sb.Append($"[head=3][color={group.Key.Color.ToHex()}][bold]{Loc.GetString(group.Key.Name)}[/bold][/color][/head]\n");

                foreach (var objEnt in group.Value)
                {
                    if (!TryComp<ObjectiveComponent>(objEnt, out var objComp))
                        continue;

                    if (component.StationMind is null)
                        continue;

                    var progress = _objectives.GetProgress(objEnt, (component.StationMind.Value, stationMind)) ?? 0;

                    var status = "cp14-objective-endtext-status-failure";
                    if (progress > 0.75f)
                        status = "cp14-objective-endtext-status-success-a";
                    if (progress > 0.99f)
                        status = "cp14-objective-endtext-status-success";

                    var meta = MetaData(objEnt);
                    sb.Append($"{Loc.GetString(objComp.LocIssuer)}: {meta.EntityName}\n");
                    sb.Append($"{Loc.GetString("cp14-objective-endtext-progress", ("value", (int)(progress * 100)))} - {Loc.GetString(status)}\n");
                }

                args.AddLine(sb.ToString());
            }
        }
    }
}
