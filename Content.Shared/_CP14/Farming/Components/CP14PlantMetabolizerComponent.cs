using Content.Shared._CP14.Farming.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Farming.Components;

/// <summary>
/// allows the plant to obtain resources by absorbing liquid from the ground
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedFarmingSystem))]
public sealed partial class CP14PlantMetabolizerComponent : Component
{
    [DataField]
    public FixedPoint2 SolutionPerUpdate = 5f;

    [DataField(required: true)]
    public ProtoId<CP14PlantMetabolizerPrototype> MetabolizerId;
}
