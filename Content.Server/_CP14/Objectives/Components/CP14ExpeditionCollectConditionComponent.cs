using Content.Server._CP14.Objectives.Systems;
using Content.Shared.Objectives;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Objectives.Components;

[RegisterComponent, Access(typeof(CP14ExpeditionCollectConditionSystem))]
public sealed partial class CP14ExpeditionCollectConditionComponent : Component
{
    [DataField]
    public ProtoId<StealTargetGroupPrototype> CollectGroup;

    /// <summary>
    /// When enabled, disables generation of this target if there is no entity on the map (disable for objects that can be created mid-round).
    /// </summary>
    [DataField]
    public bool VerifyMapExistence = true;

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

    [DataField(required: true)]
    public LocId ObjectiveText;

    [DataField(required: true)]
    public LocId DescriptionText;

    [DataField(required: true)]
    public LocId DescriptionMultiplyText;
}
