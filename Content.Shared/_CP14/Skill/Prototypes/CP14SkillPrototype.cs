using System.Numerics;
using Content.Shared._CP14.Skill.Specials;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Skill.Prototypes;

/// <summary>
/// A skill that can be learned by the player. Skills are grouped into trees, and each skill has a cost to learn, prerequisites, and an effect.
/// </summary>
[Prototype("cp14Skill")]
public sealed partial class CP14SkillPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    /// <summary>
    /// Skill Title. If you leave null, the name will try to generate from Effect.GetName()
    /// </summary>
    [DataField]
    public LocId? Name;

    /// <summary>
    /// Skill Description. If you leave null, the description will try to generate from Effect.GetDescription()
    /// </summary>
    [DataField]
    public LocId? Desc;

    /// <summary>
    /// The tree this skill belongs to. This is used to group skills together in the UI.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<CP14SkillTreePrototype> Tree = default!;

    /// <summary>
    ///  The cost to learn this skill. This is used to determine how many progress points are needed to learn the skill.
    /// </summary>
    [DataField]
    public float LearnCost = 1f;

    /// <summary>
    ///  Prerequisites for this skill. This is used to determine if the player can learn the skill. If the player does not have all the prerequisites, they cannot learn the skill.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<CP14SkillPrototype>> Prerequisites = new();

    /// <summary>
    ///  Skill UI position. This is used to determine where the skill will be displayed in the skill tree UI.
    /// </summary>
    [DataField(required: true)]
    public Vector2 SkillUiPosition = default!;

    /// <summary>
    ///  Icon for the skill. This is used to display the skill in the skill tree UI.
    /// </summary>
    [DataField(required: true)]
    public SpriteSpecifier Icon = default!;

    /// <summary>
    ///  Skill effect. This is used to determine what happens when the player learns the skill. If you leave null, the skill will not have any effect.
    ///  But the presence of the skill itself can affect some systems that check for the presence of certain skills.
    /// </summary>
    [DataField]
    public CP14SkillEffect? Effect;
}
