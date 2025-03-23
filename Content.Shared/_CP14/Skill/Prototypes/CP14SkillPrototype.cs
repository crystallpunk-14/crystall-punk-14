using Content.Shared._CP14.Skill.Specials;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Skill.Prototypes;

/// <summary>
///
/// </summary>
[Prototype("cp14Skill")]
public sealed partial class CP14SkillPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    [DataField(required: true)]
    public LocId Name;

    [DataField]
    public LocId? Description;

    [DataField(required: true)]
    public ProtoId<CP14SkillTreePrototype> Tree = default!;

    [DataField(required: true)]
    public SpriteSpecifier Icon = default!;

    [DataField(required: true)]
    public HashSet<CP14SkillEffectAddWorkbenchKnowledge> Effects = new();
}
