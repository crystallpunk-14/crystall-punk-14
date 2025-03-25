using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Skill.Components;

/// <summary>
/// Component that stores the skills learned by a player and their progress in the skill trees.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
[Access(typeof(CP14SharedSkillSystem))]
public sealed partial class CP14SkillStorageComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<ProtoId<CP14SkillPrototype>> LearnedSkills = new();

    /// <summary>
    /// Keeps track of progress points in the knowledge areas available to the player. Important: The absence of a specific area means that the player CANNOT progress in that area.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<CP14SkillTreePrototype>, FixedPoint2> LearnProgress = new();
}

/// <summary>
/// Raised when a player attempts to learn a skill. This is sent from the client to the server.
/// </summary>
[Serializable, NetSerializable]
public sealed class CP14TryLearnSkillMessage(NetEntity entity, ProtoId<CP14SkillPrototype> skill) : EntityEventArgs
{
    public readonly NetEntity Entity = entity;
    public readonly ProtoId<CP14SkillPrototype> Skill = skill;
}
