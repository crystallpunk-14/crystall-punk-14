using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill.Prototypes;

/// <summary>
/// A group of skills combined into one “branch”
/// </summary>
[Prototype("cp14SkillTree")]
public sealed partial class CP14SkillTreePrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    [DataField(required: true)]
    public LocId Name;

    [DataField]
    public LocId? Desc;

    [DataField]
    public Color Color;
}
