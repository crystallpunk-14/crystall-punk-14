using Content.Server._CP14.Objectives.Systems;
using Content.Shared.Objectives;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Objectives.Components;

[RegisterComponent, Access(typeof(CP14TownSendConditionSystem))]
public sealed partial class CP14TownSendConditionComponent : Component
{
    [DataField]
    public ProtoId<StealTargetGroupPrototype> CollectGroup;

    /// <summary>
    /// The minimum number of items you need to steal to fulfill a objective
    /// </summary>
    [DataField]
    public int MinCollectionSize = 1;

    /// <summary>
    /// The maximum number of items you need to steal to fulfill a objective
    /// </summary>
    [DataField]
    public int MaxCollectionSize = 1;

    /// <summary>
    /// Target collection size after calculation
    /// </summary>
    [DataField]
    public int CollectionSize;

    /// <summary>
    /// how many items have already been sent to the city
    /// </summary>
    [DataField]
    public int CollectionSent = 0;

    [DataField(required: true)]
    public LocId ObjectiveText;

    [DataField(required: true)]
    public LocId ObjectiveDescription;
}
