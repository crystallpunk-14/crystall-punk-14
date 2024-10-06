using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.StationZLevels;

/// <summary>
/// Prototype “floor type”. Refers to a certain group of cards united by a common style and some common rules
/// </summary>
[Prototype("zLevel")]
public sealed partial class CP14ZLevelPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    /// <summary>
    /// the name of the floor that players can see.
    /// </summary>
    [DataField(required: true)]
    public LocId Name = string.Empty;

    /// <summary>
    /// all possible floor layouts
    /// </summary>
    [DataField]
    public HashSet<ResPath> Maps = new();
}
