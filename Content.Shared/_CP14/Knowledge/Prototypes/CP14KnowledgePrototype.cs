using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Knowledge.Prototypes;

/// <summary>
/// Abstract knowledge that may be required to use items or crafts.
/// </summary>
[Prototype("CP14Knowledge")]
public sealed partial class CP14KnowledgePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Name { get; private set; }

    [DataField]
    public LocId Desc { get; private set; }

    /// <summary>
    /// To study this knowledge, other knowledge on which it is based may be necessary.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<CP14KnowledgePrototype>> Dependencies = [];
}
