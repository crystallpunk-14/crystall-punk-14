using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.LockKey;

/// <summary>
///
/// </summary>
[Prototype("CP14LockGroup")]
public sealed partial class CP14LockGroupPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;
}
