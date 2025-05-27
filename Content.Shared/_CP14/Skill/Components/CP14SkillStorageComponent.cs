using Content.Shared._CP14.ResearchTable;
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
[Access(typeof(CP14SharedSkillSystem), typeof(CP14SharedResearchSystem))]
public sealed partial class CP14SkillStorageComponent : Component
{
    /// <summary>
    /// Tracks skills that are learned without spending memory points.
    /// the skills that are here are DUBLED in the LearnedSkills, 
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<ProtoId<CP14SkillPrototype>> FreeLearnedSkills = new();

    [DataField, AutoNetworkedField]
    public List<ProtoId<CP14SkillPrototype>> LearnedSkills = new();

    /// <summary>
    /// skills that the player has learned on the research table, but has not yet learned in the skill tree.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<ProtoId<CP14SkillPrototype>> ResearchedSkills = new();

    /// <summary>
    /// The number of experience points spent on skills. Technically this could be calculated via LearnedSkills, but this is a cached value for optimization.
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 SkillsSumExperience = 0;

    /// <summary>
    /// The maximum ceiling of experience points that can be spent on learning skills. Not tied to a category.
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 ExperienceMaxCap = 5;
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
