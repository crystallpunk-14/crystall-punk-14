using Content.Shared._CP14.Workbench.Prototypes;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Workbench.Components;

[UsedImplicitly]
public sealed partial class CP14WorkbenchAddRecipeSpecial : JobSpecial
{
    [DataField(required:true), ViewVariables(VVAccess.ReadOnly)]
    public List<ProtoId<CP14WorkbenchRecipePrototype>> Recipes = new();

    public override void AfterEquip(EntityUid mob)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        var workbenchSystem = entMan.System<SharedCP14WorkbenchSystem>();
        foreach (var recipe in Recipes)
        {
            workbenchSystem.TryLearnRecipe(mob, recipe);
        }
    }
}
