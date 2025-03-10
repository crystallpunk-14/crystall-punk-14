using Content.Shared._CP14.Knowledge;
using Content.Shared._CP14.Knowledge.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Workbench.Requirements;

public sealed partial class KnowledgeRequired : CP14WorkbenchCraftRequirement
{
    /// <summary>
    /// If the player does not have this knowledge, the recipe will not be displayed in the workbench.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<CP14KnowledgePrototype> Knowledge;

    public override bool CheckRequirement(EntityManager entManager,
        IPrototypeManager protoManager,
        HashSet<EntityUid> placedEntities,
        EntityUid user)
    {
        var knowledgeSystem = entManager.System<SharedCP14KnowledgeSystem>();

        return knowledgeSystem.HasKnowledge(user, Knowledge);
    }

    public override void PostCraft(EntityManager entManager, HashSet<EntityUid> placedEntities, EntityUid user)
    {
        var knowledgeSystem = entManager.System<SharedCP14KnowledgeSystem>();
        knowledgeSystem.TryUseKnowledge(user, Knowledge);
    }

    public override string GetRequirementTitle(IPrototypeManager protoManager)
    {
        return !protoManager.TryIndex(Knowledge, out var indexedKnowledge)
            ? "Error knowledge"
            : $"{Loc.GetString("cp14-knowledge")}: {Loc.GetString(indexedKnowledge.Name)}";
    }

    public override EntityPrototype? GetRequirementEntityView(IPrototypeManager protoManager)
    {
        return null;
    }

    public override SpriteSpecifier GetRequirementTexture(IPrototypeManager protoManager)
    {
        return new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/students-cap.svg.192dpi.png"));
    }
}
