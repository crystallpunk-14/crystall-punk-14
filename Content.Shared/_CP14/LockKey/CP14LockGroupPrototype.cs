using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.LockKey;

/// <summary>
/// Group Affiliation. Used for “abstract key” mechanics,
/// where the key takes one of the free forms from identical rooms (10 different kinds of tavern rooms for example).
/// </summary>
[Prototype("CP14LockGroup")]
public sealed partial class CP14LockGroupPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;
}
