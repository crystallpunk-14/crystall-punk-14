using Content.Server._CP14.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.GameTicking.Rules;

public sealed class CP14ExpeditionObjectivesRule : GameRuleSystem<CP14ExpeditionObjectivesRuleComponent>
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawning);
    }

    private void OnPlayerSpawning(PlayerSpawnCompleteEvent args)
    {
        var query = EntityQueryEnumerator<CP14ExpeditionObjectivesRuleComponent>();
        while (query.MoveNext(out var uid, out var expedition))
        {
            if (!_mind.TryGetMind(args.Player.UserId, out var mindId, out var mind))
            {
                Log.Error($"{ToPrettyString(args.Mob):player} was trying to get the expedition objectives by {ToPrettyString(uid):rule} but had no mind attached!");
                return;
            }

            foreach (var (job, groups) in expedition.RoleObjectives)
            {
                if (args.JobId is null || args.JobId != job)
                    continue;

                foreach (var weightGroupProto in groups)
                {
                    if (!_proto.TryIndex(weightGroupProto, out var weightGroup))
                        continue;

                    _mind.TryAddObjective(mindId.Value, mind, weightGroup.Pick(_random));
                }
            }

            foreach (var (departmentProto, objectives) in expedition.DepartmentObjectives)
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
                    
                    _mind.TryAddObjective(mindId.Value, mind, weightGroup.Pick(_random));
                }
            }
        }
    }
}
