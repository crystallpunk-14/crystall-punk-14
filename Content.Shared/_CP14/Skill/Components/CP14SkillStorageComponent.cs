using Content.Shared._CP14.Skill.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill.Components;

/// <summary>
/// This entity can be used to craft other objects through the interface
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(CP14SkillSystem))]
public sealed partial class CP14SkillStorageComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<ProtoId<CP14SkillPrototype>> Skills = new();
}
