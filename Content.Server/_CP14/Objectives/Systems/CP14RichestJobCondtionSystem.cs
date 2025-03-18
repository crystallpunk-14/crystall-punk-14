using Content.Server._CP14.Objectives.Components;
using Content.Shared._CP14.Currency;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Objectives.Systems;

public sealed class CP14RichestJobConditionSystem : EntitySystem
{
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;
    [Dependency] private readonly CP14SharedCurrencySystem _currency = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedJobSystem _job = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14RichestJobConditionComponent, ObjectiveAfterAssignEvent>(OnCollectAfterAssign);
        SubscribeLocalEvent<CP14RichestJobConditionComponent, ObjectiveGetProgressEvent>(OnCollectGetProgress);
    }

    private void OnCollectAfterAssign(Entity<CP14RichestJobConditionComponent> condition, ref ObjectiveAfterAssignEvent args)
    {
        if (!_proto.TryIndex(condition.Comp.Job, out var indexedJob))
            return;

        _metaData.SetEntityName(condition.Owner, Loc.GetString(condition.Comp.ObjectiveText), args.Meta);
        _metaData.SetEntityDescription(condition.Owner, Loc.GetString(condition.Comp.ObjectiveDescription), args.Meta);
        _objectives.SetIcon(condition.Owner, condition.Comp.ObjectiveSprite);
    }

    private void OnCollectGetProgress(Entity<CP14RichestJobConditionComponent> condition, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = GetProgress(args.MindId, args.Mind, condition);
    }

    private float GetProgress(EntityUid mindId, MindComponent mind, CP14RichestJobConditionComponent condition)
    {
        if (mind.OwnedEntity is null)
            return 0;

        var ourValue = _currency.GetTotalCurrencyRecursive(mind.OwnedEntity.Value);
        var otherMaxValue = 0;

        var allHumans = _mind.GetAliveHumans(mindId);
        if (allHumans.Count == 0)
            return 1; // No one to compare to, so we're the richest.

        foreach (var otherHuman in allHumans)
        {
            if (!_job.MindTryGetJob(otherHuman, out var otherJob))
                continue;

            if (otherJob != condition.Job)
                continue;

            if (otherHuman.Comp.OwnedEntity is null)
                continue;

            var otherValue = _currency.GetTotalCurrencyRecursive(otherHuman.Comp.OwnedEntity.Value);
            if (otherValue > otherMaxValue)
                otherMaxValue = otherValue;
        }

        // if several players have the same amount of money, no one wins.
        return ourValue == otherMaxValue ? 0.99f : Math.Clamp(ourValue / (float)otherMaxValue, 0, 1);
    }
}
