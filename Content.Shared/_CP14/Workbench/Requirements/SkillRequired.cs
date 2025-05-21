using Content.Shared._CP14.Skill;
using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared._CP14.Workbench.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Workbench.Requirements;

public sealed partial class SkillRequired : CP14WorkbenchCraftRequirement
{
    public override bool HideRecipe { get; set; } = true;

    [DataField(required: true)]
    public List<ProtoId<CP14SkillPrototype>> Skills = new();

    public override bool CheckRequirement(EntityManager entManager,
        IPrototypeManager protoManager,
        HashSet<EntityUid> placedEntities,
        EntityUid user)
    {
        var knowledgeSystem = entManager.System<CP14SharedSkillSystem>();

        var haveAllSkills = true;
        foreach (var skill in Skills)
        {
            if (!knowledgeSystem.HaveSkill(user, skill))
            {
                haveAllSkills = false;
                break;
            }
        }

        return haveAllSkills;
    }

    public override void PostCraft(EntityManager entManager, IPrototypeManager protoManager, HashSet<EntityUid> placedEntities, EntityUid user)
    {
    }

    public override string GetRequirementTitle(IPrototypeManager protoManager)
    {
        return string.Empty;
    }

    public override EntityPrototype? GetRequirementEntityView(IPrototypeManager protoManager)
    {
        return null;
    }

    public override SpriteSpecifier? GetRequirementTexture(IPrototypeManager protoManager)
    {
        return null;
    }
}
