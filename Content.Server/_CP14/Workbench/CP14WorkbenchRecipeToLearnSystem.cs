using Content.Shared.Interaction.Events;
using Content.Server._CP14.Workbench;
using Content.Shared._CP14.Workbench;


namespace Content.Server._CP14.Workbench;

public sealed partial class CP14WorkbenchRecipeSystem : EntitySystem
{
    [Dependency] private readonly SharedCP14WorkbenchSystem _workbench = default!;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CP14WorkbenchRecipeToLearnComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnUseInHand(Entity<CP14WorkbenchRecipeToLearnComponent> ent, ref UseInHandEvent args)
    {
        if (!TryComp<CP14WorkbenchRecipeToLearnComponent>(ent, out var recipeComp))
            return;

        bool result = _workbench.TryLearnRecipe(args.User, recipeComp.Recipe);
    }
}
