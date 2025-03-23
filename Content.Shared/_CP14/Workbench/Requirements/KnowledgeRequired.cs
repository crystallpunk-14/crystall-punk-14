using Content.Shared._CP14.Workbench.Prototypes;
using Content.Shared._CP14.WorkbenchKnowledge;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Workbench.Requirements;

public sealed partial class KnowledgeRequired : CP14WorkbenchCraftRequirement
{
    public override bool HideRecipe { get; set; } = true;

    public override bool CheckRequirement(EntityManager entManager,
        IPrototypeManager protoManager,
        HashSet<EntityUid> placedEntities,
        EntityUid user,
        CP14WorkbenchRecipePrototype recipe)
    {
        var knowledgeSystem = entManager.System<SharedCP14WorkbenchKnowledgeSystem>();

        return knowledgeSystem.HasKnowledge(user, recipe);
    }

    public override void PostCraft(EntityManager entManager, HashSet<EntityUid> placedEntities, EntityUid user)
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
