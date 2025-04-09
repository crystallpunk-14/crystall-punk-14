using Content.Shared.Destructible.Thresholds;
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
    /// The difficulty levels at which this modifier can be generated.
    /// </summary>
    [DataField]
    public MinMax Levels = new(1, 10);

    /// <summary>
    /// Each modifier belongs to specific categories. Used by the generator to determine what to generate
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<CP14DemiplaneModifierCategoryPrototype>, float> Categories = new();

    /// <summary>
    /// How often can this modifier be generated? Determined by weight from all modifiers available for the location
    /// </summary>
    [DataField]
    public float GenerationWeight = 1;

    /// <summary>
    /// If this modifier is chosen to be generated - it can simply be ignored with some chance.
    /// This is useful, for example, for the Fun category. According to the basic logic it should always be filled,
    /// but thanks to this field, we can just have a chance that nothing will be generated.
    /// </summary>
    [DataField]
    public float GenerationProb = 1f;

    /// <summary>
    /// Can this modifier be generated multiple times within a single demiplane?
    /// </summary>
    [DataField]
    public bool Unique = true;

    /// <summary>
    /// Generation layers that will be added to the location generation after the main layers.
    /// </summary>
    [DataField]
    public List<IDunGenLayer>? Layers;

    /// <summary>
    /// Components that will be added to the map
    /// </summary>
    [DataField]
    public ComponentRegistry? Components;

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
