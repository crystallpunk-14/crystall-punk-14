using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skills;

/// <summary>
/// Limits the use of this entity behind certain skills
/// </summary>
[RegisterComponent, Access(typeof(CP14SkillSystem))]
public sealed partial class CP14SkillRequirementComponent : Component
{
    /// <summary>
    /// Is it necessary to have ALL the skills on the list to be able to use this entity?
    /// If not, one of any skill will suffice
    /// </summary>
    [DataField]
    public bool NeedAll = false;

    [DataField(required: true)]
    public List<ProtoId<CP14SkillPrototype>> RequiredSkills = new();
}
