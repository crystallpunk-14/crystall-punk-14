using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.LockKey;

/// <summary>
/// A prototype of the lock category. Need a roundstart mapping to ensure that keys and locks will fit together despite randomization.
/// </summary>
[Prototype("CP14LockCategory")]
public sealed partial class CP14LockCategoryPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The number of elements that will be generated for the category.
    /// </summary>
    [DataField] public int Complexity = 3;
}
