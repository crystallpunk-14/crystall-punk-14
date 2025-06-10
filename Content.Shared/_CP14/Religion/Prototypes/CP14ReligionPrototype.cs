using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Religion.Prototypes;

/// <summary>
///
/// </summary>
[Prototype("cp14Religion")]
public sealed partial class CP14ReligionPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    [DataField]
    public float FollowerObservationRadius = 8f;

    [DataField]
    public float AltarObservationRadius = 25f;
}
