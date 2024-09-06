using Content.Server.Popups;
using Content.Shared.Interaction;
using Content.Server._CP14.Workbench.WorkbenchRecipe;
using Content.Server._CP14.Workbench;


namespace Content.Server._CP14.Workbench;

public sealed partial class CP14WorkbenchRecipeSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly CP14WorkbenchSystem _workbench = default!;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CP14WorkbenchRecipeComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(Entity<CP14WorkbenchRecipeComponent> ent, ref AfterInteractEvent args)
    {
        if (!TryComp<CP14WorkbenchRecipeComponent>(ent, out var recipeComp))
            return;

        bool result = _workbench.AddRecipe(args.Target, recipeComp.Recipe, args.ClickLocation);
        if (result)
            Del(ent);
    }
}
