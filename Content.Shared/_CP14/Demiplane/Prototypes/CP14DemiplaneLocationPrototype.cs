using Content.Shared.Procedural;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Demiplane.Prototypes;

/// <summary>
/// procedural location template. The answer to the question “Where” as far as the combinatorics of the expedition is concerned.
/// </summary>
[Prototype("cp14DemiplaneLocation")]
public sealed partial class CP14DemiplaneLocationPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    /// <summary>
    /// Location Tier. Can be generated only in demiplane keys with the corresponding tier
    /// </summary>
    [DataField]
    public int Tier = 1;

    [DataField(required: true)]
    public ProtoId<DungeonConfigPrototype> LocationConfig;

    /// <summary>
    /// Components that will be automatically added to the demiplane when it is generated
    /// </summary>
    [DataField]
    public ComponentRegistry Components = new();

    /// <summary>
    /// Tags allow modifiers to filter which ones can apply to the current location and which ones cannot.
    /// </summary>
    [DataField]
    public List<ProtoId<TagPrototype>> Tags = new();

    [DataField]
    public LocId? Name;

    [DataField]
    public float ExamineProb = 0.75f;
}
