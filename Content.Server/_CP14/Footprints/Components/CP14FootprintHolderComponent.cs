using Content.Shared.Decals;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Footprints.Components;

/// <summary>
///  stores the type of footprints and their settings.
/// </summary>
[RegisterComponent, Access(typeof(CP14FootprintsSystem))]
public sealed partial class CP14FootprintHolderComponent : Component
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
    public float Intensity = 0f;

    [DataField]
    public FixedPoint2 PickSolution = 1f;

    [DataField]
    public float DistanceIntensityCost = 0.2f;
}
