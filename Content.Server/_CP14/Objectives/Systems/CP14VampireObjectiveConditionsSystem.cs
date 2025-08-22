using Content.Server._CP14.Objectives.Components;
using Content.Server.Station.Components;
using Content.Shared._CP14.Vampire.Components;
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
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedJobSystem _jobs = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public readonly float RequiredAlivePercentage = 0.5f;

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
        _meta.SetEntityDescription(ent, Loc.GetString("cp14-objective-vampire-defence-settlement-desc", ("count", RequiredAlivePercentage * 100)));
        _objectives.SetIcon(ent, ent.Comp.Icon);
    }

    public float CalculateAlivePlayersPercentage()
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

        return totalPlayers > 0 ? (alivePlayers / totalPlayers) : 0f;
    }

    private void OnDefenceGetProgress(Entity<CP14VampireDefenceVillageConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = CalculateAlivePlayersPercentage() > RequiredAlivePercentage ? 1 : 0;
    }

    private void OnBloodPurityAfterAssign(Entity<CP14VampireBloodPurityConditionComponent> ent, ref ObjectiveAfterAssignEvent args)
    {
         if (!TryComp<CP14VampireComponent>(args.Mind?.OwnedEntity, out var vampireComp))
             return;

         ent.Comp.Faction = vampireComp.Faction;

         _meta.SetEntityName(ent, Loc.GetString("cp14-objective-vampire-pure-bood-title"));
         _meta.SetEntityDescription(ent, Loc.GetString("cp14-objective-vampire-pure-bood-desc"));
         _objectives.SetIcon(ent, ent.Comp.Icon);
    }

    private void OnBloodPurityGetProgress(Entity<CP14VampireBloodPurityConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        var query = EntityQueryEnumerator<CP14VampireComponent, MobStateComponent>();

        while (query.MoveNext(out var uid, out var vampire, out var mobState))
        {
            if (vampire.Faction != ent.Comp.Faction)
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
