using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Skill.Components;

/// <summary>
///
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
[Access(typeof(CP14SharedSkillSystem))]
public sealed partial class CP14SkillStorageComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<ProtoId<CP14SkillPrototype>> LearnedSkills = new();

    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<CP14SkillTreePrototype>, FixedPoint2> LearnProgress = new();
}


[Serializable, NetSerializable]
public sealed class CP14TryLearnSkillMessage(NetEntity entity, ProtoId<CP14SkillPrototype> skill) : EntityEventArgs
{
    public readonly NetEntity Entity = entity;
    public readonly ProtoId<CP14SkillPrototype> Skill = skill;
}
