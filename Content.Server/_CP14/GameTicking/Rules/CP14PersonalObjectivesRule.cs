using System.Text;
using Content.Server._CP14.GameTicking.Rules.Components;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
using Content.Shared.Random.Helpers;
using Content.Shared.Roles.Jobs;
using Robust.Server.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.GameTicking.Rules;

public sealed class CP14PersonalObjectivesRule : GameRuleSystem<CP14PersonalObjectivesRuleComponent>
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedJobSystem _jobs = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawning);
    }

    private void OnPlayerSpawning(PlayerSpawnCompleteEvent args)
    {
        var query = EntityQueryEnumerator<CP14PersonalObjectivesRuleComponent>();
        while (query.MoveNext(out var uid, out var personalObj))
        {
            if (!_mind.TryGetMind(args.Player.UserId, out var mindId, out var mind))
            {
                Log.Error(
                    $"{ToPrettyString(args.Mob):player} was trying to get the expedition objectives by {ToPrettyString(uid):rule} but had no mind attached!");
                return;
            }

            foreach (var (job, groups) in personalObj.RoleObjectives)
            {
                if (args.JobId is null || args.JobId != job)
                    continue;

                foreach (var weightGroupProto in groups)
                {
                    if (!_proto.TryIndex(weightGroupProto, out var weightGroup))
                        continue;

                    _mind.TryAddObjective(mindId.Value, mind, weightGroup.Pick(_random), out var objective);

                    if (objective is not null)
                    {
                        if (!personalObj.PersonalObjectives.ContainsKey(mindId.Value))
                            personalObj.PersonalObjectives.Add(mindId.Value, new());

                        personalObj.PersonalObjectives[mindId.Value].Add(objective.Value);
                    }
                }
            }

            foreach (var (departmentProto, objectives) in personalObj.DepartmentObjectives)
            {
                if (args.JobId is null)
                    continue;

                if (!_proto.TryIndex(departmentProto, out var department))
                    continue;

                if (!department.Roles.Contains(args.JobId))
                    continue;

                foreach (var weightGroupProto in objectives)
                {
                    if (!_proto.TryIndex(weightGroupProto, out var weightGroup))
                        continue;

                    _mind.TryAddObjective(mindId.Value, mind, weightGroup.Pick(_random), out var objective);

                    if (objective is not null)
                    {
                        if (!personalObj.PersonalObjectives.ContainsKey(mindId.Value))
                            personalObj.PersonalObjectives.Add(mindId.Value, new());

                        personalObj.PersonalObjectives[mindId.Value].Add(objective.Value);
                    }
                }
            }
        }
    }

    protected override void AppendRoundEndText(EntityUid uid,
        CP14PersonalObjectivesRuleComponent component,
        GameRuleComponent gameRule,
        ref RoundEndTextAppendEvent args)
    {
        base.AppendRoundEndText(uid, component, gameRule, ref args);

        var sb = new StringBuilder();
        sb.Append($"[head=2]{Loc.GetString("cp14-objective-issuer-personal")}[/head]\n");

        foreach (var (mind, objectives) in component.PersonalObjectives)
        {
            if (!TryComp<MindComponent>(mind, out var mindComp))
                continue;

            var name = mindComp.CharacterName ?? Loc.GetString("cp14-objective-unknown");
            var role = Loc.GetString("cp14-objective-unknown");
            var ckey = Loc.GetString("cp14-objective-unknown");

            if (_jobs.MindTryGetJob(mind, out var job))
                role = Loc.GetString(job.Name);

            if (mindComp.UserId is not null)
            {
                ckey = _player.GetPlayerData(mindComp.UserId.Value).UserName;
            }

            //We dont show players without completed objectives
            var show = false;
            var sbLocal = new StringBuilder();
            sbLocal.Append($"[head=3]{name} - {role}[/head]\n");
            sbLocal.Append($"[color=#949494]{ckey}[/color]\n");
            foreach (var objEnt in objectives)
            {
                if (!HasComp<ObjectiveComponent>(objEnt))
                    continue;

                var progress = _objectives.GetProgress(objEnt, (mind, mindComp)) ?? 0;

                if (progress <= 0.75f)
                    continue;

                var status = "cp14-objective-endtext-status-failure";
                if (progress > 0.75f)
                    status = "cp14-objective-endtext-status-success-a";
                if (progress > 0.99f)
                    status = "cp14-objective-endtext-status-success";

                show = true;

                var meta = MetaData(objEnt);
                sbLocal.Append($"{meta.EntityName} - {Loc.GetString(status)} ({(int)(progress * 100)}%)\n");
            }

            if (show)
                sb.Append(sbLocal);
        }

        args.AddLine(sb.ToString());
    }
}
