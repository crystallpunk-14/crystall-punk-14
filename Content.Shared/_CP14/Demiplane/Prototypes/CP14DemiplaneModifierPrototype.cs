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
    public HashSet<ProtoId<TagPrototype>> BlacklistTags = new();

    /// <summary>
    /// Modifier can only be applied to locations that have all of the following tags. Leave empty for disable
    /// </summary>
    public HashSet<ProtoId<TagPrototype>> RequiredTags = new();
}
