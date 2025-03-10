using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.UniqueLoot;

/// <summary>
/// Defines a unique loot that can only be generated a specified number of times per round.
/// </summary>
[Prototype("uniqueSpawn")]
public sealed partial class CP14UniqueLootPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// ProtoId of the entity to spawn.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Entity { get; private set; }

    /// <summary>
    /// Count of how many times this loot can be generated per round.
    /// </summary>
    [DataField]
    public int Count { get; private set; } = 1;

    /// <summary>
    /// Used for categorizing unique loot. Used in spawners to determine which loot to spawn.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<TagPrototype>> Tags { get; private set; } = new();
}
