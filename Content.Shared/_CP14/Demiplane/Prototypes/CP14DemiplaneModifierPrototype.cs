using Content.Shared.Procedural;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Demiplane.Prototypes;

/// <summary>
/// Demiplane modifier prototype. The answer to the question “Which” in terms of the combinatorics of demiplane generation is
/// </summary>
[Prototype("cp14DemiplaneModifier")]
public sealed partial class CP14DemiplaneModifierPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    /// <summary>
    /// Each modifier belongs to specific categories. Used by the generator to determine what to generate
    /// </summary>
    [DataField(required: true)]
    public List<ProtoId<CP14DemiplaneModifierCategoryPrototype>> Categories = new();

    /// <summary>
    /// Abstract danger of this modifier. The demiplane has a threat limit, which it gains from modifiers until it reaches the limit.
    /// </summary>
    [DataField]
    public float Difficulty = 0;

    /// <summary>
    /// The abstract value of this modifier. The demiplane has a limit of rewards it gains from modifiers until it reaches the limit.
    /// </summary>
    [DataField]
    public float Reward = 0;

    /// <summary>
    /// How often can this modifier be generated? Determined by weight from all modifiers available for the location
    /// </summary>
    [DataField]
    public float GenerationWeight = 1;

    /// <summary>
    /// Can this modifier be generated multiple times within a single demiplane?
    /// </summary>
    [DataField]
    public bool Unique = true;

    /// <summary>
    /// Generation layers that will be added to the location generation after the main layers.
    /// </summary>
    [DataField]
    public List<IDunGenLayer> Layers = new();

    /// <summary>
    /// Components that will be added to the map
    /// </summary>
    [DataField]
    public ComponentRegistry Components = new();

    /// <summary>
    /// Modifier cannot be applied to locations with the following tags. Leave empty for disable
    /// </summary>
    [DataField]
    public List<ProtoId<TagPrototype>> BlacklistTags = new();

    /// <summary>
    /// Modifier can only be applied to locations that have all of the following tags. Leave empty for disable
    /// </summary>
    [DataField]
    public List<ProtoId<TagPrototype>> RequiredTags = new();

    [DataField]
    public LocId? Name;

    [DataField]
    public float ExamineProb = 0.75f;
}
