using Content.Shared.Decals;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.FootStep;

/// <summary>
///  allows an entity to leave footprints on the tiles
/// </summary>
[RegisterComponent, Access(typeof(CP14FootStepSystem))]
public sealed partial class CP14FootStepComponent : Component
{
    [DataField]
    public ProtoId<DecalPrototype> DecalProto = "CP14FootprintsBoots";

    [DataField]
    public float DecalDistance = 1f;

    [DataField]
    public float DistanceTraveled = 0f;

    [DataField]
    public Color DecalColor = Color.White;

    [DataField]
    public float Intensity = 1f;

    [DataField]
    public FixedPoint2 PickSolution = 1f;

    [DataField]
    public float DistanceIntensityCost = 0.2f;
}
