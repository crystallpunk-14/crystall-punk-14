using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Demiplane.Prototypes;

/// <summary>
///
/// </summary>
[Prototype("cp14DemiplaneModifierCategory")]
public sealed partial class CP14DemiplaneModifierCategoryPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;
}
