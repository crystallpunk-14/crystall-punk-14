using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.LockKey;

/// <summary>
///
/// </summary>
[Prototype("CP14LockType")]
public sealed partial class CP14LockTypePrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The number of elements that will be generated for the category.
    /// </summary>
    [DataField]
    public int Complexity = 3;

    /// <summary>
    ///
    /// </summary>
    [DataField]
    public ProtoId<CP14LockGroupPrototype>? Group;
}
