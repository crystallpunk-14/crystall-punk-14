using Content.Shared._CP14.Skills.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skills.Components;

/// <summary>
/// Limits the use of this entity behind certain skills
/// </summary>
[RegisterComponent, Access(typeof(SharedCP14SkillSystem))]
public sealed partial class CP14SkillRequirementComponent : Component
{
    /// <summary>
    /// Is it necessary to have ALL the skills on the list to be able to use this entity?
    /// If not, one of any skill will suffice
    /// </summary>
    [DataField]
    public bool NeedAll = false;

    /// <summary>
    /// the chances of something going wrong when using wihout skill
    /// </summary>
    [DataField]
    public float FuckupChance = 0.5f;

    [DataField(required: true)]
    public List<ProtoId<CP14SkillPrototype>> RequiredSkills = new();
}
