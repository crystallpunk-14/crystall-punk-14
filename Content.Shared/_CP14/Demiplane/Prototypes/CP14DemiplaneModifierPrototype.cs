using Content.Shared.Procedural;
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
}
