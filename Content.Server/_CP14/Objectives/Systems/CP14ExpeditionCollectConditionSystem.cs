using Content.Server._CP14.Objectives.Components;
using Content.Server._CP14.Shuttles;
using Content.Server.Objectives.Components.Targets;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.Objectives.Systems;

public sealed class CP14ExpeditionCollectConditionSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;
    [Dependency] private readonly CP14ExpeditionSystem _cp14Expedition = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14ExpeditionCollectConditionComponent, ObjectiveAssignedEvent>(OnAssigned);
        SubscribeLocalEvent<CP14ExpeditionCollectConditionComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
        SubscribeLocalEvent<CP14ExpeditionCollectConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnAssigned(Entity<CP14ExpeditionCollectConditionComponent> condition, ref ObjectiveAssignedEvent args)
    {
        List<StealTargetComponent?> targetList = new();

        var query = AllEntityQuery<StealTargetComponent>();
        while (query.MoveNext(out var target))
        {
            if (condition.Comp.CollectGroup != target.StealGroup)
                continue;

            targetList.Add(target);
        }

        // cancel if the required items do not exist
        if (targetList.Count == 0 && condition.Comp.VerifyMapExistence)
        {
            args.Cancelled = true;
            return;
        }

        //setup condition settings
        var maxSize = condition.Comp.VerifyMapExistence
            ? Math.Min(targetList.Count, condition.Comp.MaxCollectionSize)
            : condition.Comp.MaxCollectionSize;
        var minSize = condition.Comp.VerifyMapExistence
            ? Math.Min(targetList.Count, condition.Comp.MinCollectionSize)
            : condition.Comp.MinCollectionSize;

        condition.Comp.CollectionSize = _random.Next(minSize, maxSize);
    }

    //Set the visual, name, icon for the objective.
    private void OnAfterAssign(Entity<CP14ExpeditionCollectConditionComponent> condition, ref ObjectiveAfterAssignEvent args)
    {
        var group = _proto.Index(condition.Comp.CollectGroup);

        var title = Loc.GetString(condition.Comp.ObjectiveText, ("itemName", group.Name));

        var description = condition.Comp.CollectionSize > 1
            ? Loc.GetString(condition.Comp.DescriptionMultiplyText, ("itemName", group.Name), ("count", condition.Comp.CollectionSize))
            : Loc.GetString(condition.Comp.DescriptionText, ("itemName", group.Name));

        _metaData.SetEntityName(condition.Owner, title, args.Meta);
        _metaData.SetEntityDescription(condition.Owner, description, args.Meta);
        _objectives.SetIcon(condition.Owner, group.Sprite, args.Objective);
    }

    private void OnGetProgress(Entity<CP14ExpeditionCollectConditionComponent> condition, ref ObjectiveGetProgressEvent args)
    {
        var count = 0;

        if (!_cp14Expedition.TryGetExpeditionShip(out var ship))
            return;

        var query = EntityQueryEnumerator<StealTargetComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            if (Transform(uid).GridUid != Transform(ship!.Value).GridUid)
                continue;

            if (CheckStealTarget(uid, condition))
                count++;
        }

        var result = count / (float) condition.Comp.CollectionSize;
        result = Math.Clamp(result, 0, 1);
        args.Progress = result;
    }

    private bool CheckStealTarget(EntityUid entity, CP14ExpeditionCollectConditionComponent condition)
    {
        // check if this is the target
        if (!TryComp<StealTargetComponent>(entity, out var target))
            return false;

        if (target.StealGroup != condition.CollectGroup)
            return false;

        return true;
    }
}
