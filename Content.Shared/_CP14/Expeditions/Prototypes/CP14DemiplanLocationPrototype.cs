using Content.Shared.Procedural;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Expeditions.Prototypes;

/// <summary>
/// procedural location template. The answer to the question “Where” as far as the combinatorics of the expedition is concerned.
/// </summary>
[Prototype("cp14DemiplanLocation")]
public sealed partial class CP14DemiplanLocationPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    [DataField(required: true)]
    public ProtoId<DungeonConfigPrototype> LocationConfig;

    [DataField]
    public ComponentRegistry Components = new();

    //Player faced description

    //Tier restriction

    //Some abstract restriction?
}
