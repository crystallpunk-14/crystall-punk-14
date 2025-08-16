using Content.Server._CP14.Objectives.Components;
using Content.Server.Station.Components;
using Content.Shared._CP14.Vampire;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Objectives.Systems;

public sealed class CP14VampireObjectiveConditionsSystem : EntitySystem
{
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedJobSystem _jobs = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14VampireBloodPurityConditionComponent, ObjectiveAfterAssignEvent>(OnBloodPurityAfterAssign);
        SubscribeLocalEvent<CP14VampireBloodPurityConditionComponent, ObjectiveGetProgressEvent>(OnBloodPurityGetProgress);

        SubscribeLocalEvent<CP14VampireDefenceVillageConditionComponent, ObjectiveAfterAssignEvent>(OnDefenceAfterAssign);
        SubscribeLocalEvent<CP14VampireDefenceVillageConditionComponent, ObjectiveGetProgressEvent>(OnDefenceGetProgress);
    }

    private void OnDefenceAfterAssign(Entity<CP14VampireDefenceVillageConditionComponent> ent, ref ObjectiveAfterAssignEvent args)
    {
        _meta.SetEntityName(ent, Loc.GetString("cp14-objective-vampire-defence-settlement-title"));
        _meta.SetEntityDescription(ent, Loc.GetString("cp14-objective-vampire-defence-settlement-desc", ("count", ent.Comp.DefencePercentage * 100)));
        _objectives.SetIcon(ent, ent.Comp.Icon);
    }

    private void OnDefenceGetProgress(Entity<CP14VampireDefenceVillageConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        var query = EntityQueryEnumerator<StationJobsComponent>();

        var totalPlayers = 0f;
        var alivePlayers = 0f;
        while (query.MoveNext(out var uid, out var jobs))
        {
            totalPlayers += jobs.PlayerJobs.Count;

            foreach (var (netUserId, jobsList) in jobs.PlayerJobs)
            {
                if (!_mind.TryGetMind(netUserId, out var mind))
                    continue;

                if (!_jobs.MindTryGetJob(mind, out var jobRole))
                    continue;

                var firstMindEntity = GetEntity(mind.Value.Comp.OriginalOwnedEntity);

                if (firstMindEntity is null)
                    continue;

                if (!_mobState.IsDead(firstMindEntity.Value))
                    alivePlayers++;
            }
        }

        args.Progress = (alivePlayers / totalPlayers) > ent.Comp.DefencePercentage ? 1 : 0;
    }

    private void OnBloodPurityAfterAssign(Entity<CP14VampireBloodPurityConditionComponent> ent, ref ObjectiveAfterAssignEvent args)
    {
         if (!TryComp<CP14VampireComponent>(args.Mind?.OwnedEntity, out var vampireComp))
             return;

         ent.Comp.Faction = vampireComp.FactionIcon;

         _meta.SetEntityName(ent, Loc.GetString("cp14-objective-vampire-pure-bood-title"));
         _meta.SetEntityDescription(ent, Loc.GetString("cp14-objective-vampire-pure-bood-desc"));
         _objectives.SetIcon(ent, ent.Comp.Icon);
    }

    private void OnBloodPurityGetProgress(Entity<CP14VampireBloodPurityConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        var queue = EntityQueryEnumerator<CP14VampireComponent, MobStateComponent>();

        while (queue.MoveNext(out var uid, out var vampire, out var mobState))
        {
            if (vampire.FactionIcon != ent.Comp.Faction)
            {
                if (mobState.CurrentState == MobState.Dead)
                    continue;

                args.Progress = 0f;
                return;
            }
        }

        args.Progress = 1f;
    }
}
