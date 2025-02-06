using Content.Server._CP14.Objectives.Components;
using Content.Server._CP14.RoundStatistic;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.Objectives.Systems;

public sealed class CP14StatisticRangeConditionSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;
    [Dependency] private readonly CP14RoundStatTrackerSystem _statistic = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14StatisticRangeConditionComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
        SubscribeLocalEvent<CP14StatisticRangeConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnAfterAssign(Entity<CP14StatisticRangeConditionComponent> condition, ref ObjectiveAfterAssignEvent args)
    {
        var title = Loc.GetString(condition.Comp.ObjectiveText,
            ("min", condition.Comp.Range.Min),
            ("max", condition.Comp.Range.Max));

        var description = Loc.GetString(condition.Comp.ObjectiveDescription,
            ("min", condition.Comp.Range.Min),
            ("max", condition.Comp.Range.Max));

        _metaData.SetEntityName(condition.Owner, title, args.Meta);
        _metaData.SetEntityDescription(condition.Owner, description, args.Meta);
        if (condition.Comp.ObjectiveSprite is not null)
            _objectives.SetIcon(condition.Owner, condition.Comp.ObjectiveSprite, args.Objective);
    }

    private void OnGetProgress(Entity<CP14StatisticRangeConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        var statValue = _statistic.GetTrack(ent.Comp.Statistic);

        if (statValue is null || statValue > ent.Comp.Range.Max || statValue < ent.Comp.Range.Min)
        {
            args.Progress = 0;
            return;
        }

        args.Progress = 1;
    }
}
