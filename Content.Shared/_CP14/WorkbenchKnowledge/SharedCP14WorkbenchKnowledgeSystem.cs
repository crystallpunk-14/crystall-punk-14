using Content.Shared._CP14.Workbench.Prototypes;
using Content.Shared.Ghost;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.WorkbenchKnowledge;

public abstract class SharedCP14WorkbenchKnowledgeSystem : EntitySystem
{
    public bool HasKnowledge(Entity<Components.CP14WorkbenchKnowledgeStorageComponent?> entity, ProtoId<CP14WorkbenchRecipePrototype> knowledge)
    {
        if (HasComp<GhostComponent>(entity))
            return true;

        return Resolve(entity, ref entity.Comp, false) && entity.Comp.Recipes.Contains(knowledge);
    }
}
