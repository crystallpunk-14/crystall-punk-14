using Content.Shared._CP14.Workbench.Prototypes;
using Content.Shared._CP14.WorkbenchKnowledge;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill.Specials;

public sealed partial class CP14SkillEffectAddWorkbenchKnowledge : CP14SkillEffect
{
    [DataField]
    public List<ProtoId<CP14WorkbenchRecipePrototype>> Recipes = new();

    public override void AddSkill(EntityManager entManager, EntityUid target)
    {
        var knowledgeSystem = entManager.System<SharedCP14WorkbenchKnowledgeSystem>();
        foreach (var recipe in Recipes)
        {
            knowledgeSystem.TryAdd(target, recipe);
        }
    }

    public override void RemoveSkill(EntityManager entManager, EntityUid target)
    {
        var knowledgeSystem = entManager.System<SharedCP14WorkbenchKnowledgeSystem>();
        foreach (var recipe in Recipes)
        {
            knowledgeSystem.TryRemove(target, recipe);
        }
    }
}
