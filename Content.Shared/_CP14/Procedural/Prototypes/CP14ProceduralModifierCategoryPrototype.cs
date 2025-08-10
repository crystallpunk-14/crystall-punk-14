using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Procedural.Prototypes;

/// <summary>
///
/// </summary>
[Prototype("cp14LocationModifierCategory")]
public sealed partial class CP14ProceduralModifierCategoryPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;
}
