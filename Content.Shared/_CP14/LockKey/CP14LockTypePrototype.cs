using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.LockKey;

/// <summary>
/// A lock or key shape, pre-generated at the start of the round.
/// Allows a group of doors and keys to have the same shape within the same round and fit together,
/// but is randomized from round to round
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
    /// Group Affiliation. Used for “abstract key” mechanics,
    /// where the key takes one of the free forms from identical rooms (10 different kinds of tavern rooms for example).
    /// </summary>
    [DataField]
    public ProtoId<CP14LockGroupPrototype>? Group;

    [DataField]
    public LocId? Name;
}
