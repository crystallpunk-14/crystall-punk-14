using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skills.Prototypes;

/// <summary>
///
/// </summary>
[Prototype("CP14Skill")]
public sealed partial class CP14SkillPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId? Name{ get; private set; }

    [DataField]
    public LocId? Desc{ get; private set; }

    [DataField]
    public ComponentRegistry Components = new();
}
