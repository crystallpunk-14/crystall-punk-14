using Content.Shared._CP14.Skills.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skills.Components;

/// <summary>
/// The ability to add a skill to an entity and quickly teach it some skills
/// </summary>
[RegisterComponent, Access(typeof(SharedCP14SkillSystem))]
public sealed partial class CP14AutoAddSkillComponent : Component
{
    [DataField]
    public List<ProtoId<CP14SkillPrototype>> Skills = new();
}
