using Content.Server._CP14.Objectives.Systems;
using Content.Shared._CP14.RoundStatistic;
using Content.Shared.Destructible.Thresholds;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._CP14.Objectives.Components;

[RegisterComponent, Access(typeof(CP14StatisticRangeConditionSystem))]
public sealed partial class CP14StatisticRangeConditionComponent : Component
{
    [DataField(required: true)]
    public ProtoId<CP14RoundStatTrackerPrototype> Statistic;

    [DataField(required: true)]
    public MinMax Range;

    [DataField(required: true)]
    public LocId ObjectiveText;

    [DataField(required: true)]
    public LocId ObjectiveDescription;

    [DataField(required: true)]
    public SpriteSpecifier? ObjectiveSprite;
}
