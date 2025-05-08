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
    /// The number of experience points spent on skills. Technically this could be calculated via LearnedSkills, but this is a cached value for optimization.
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 SkillsSumExperience = 0;

    /// <summary>
    /// Keeps track of progress points in the knowledge areas available to the player. Important: The absence of a specific area means that the player CANNOT progress in that area.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<CP14SkillTreePrototype>, FixedPoint2> Progress = new();

    /// <summary>
    /// The maximum ceiling of experience points that can be spent on learning skills. Not tied to a category.
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 ExperienceMaxCap = 10;
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
