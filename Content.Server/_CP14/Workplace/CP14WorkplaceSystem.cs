using Content.Shared._CP14.Workplace;
using Content.Shared.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Workplace;

public sealed partial class CP14WorkplaceSystem : CP14SharedWorkbenchSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypeReload);
        SubscribeLocalEvent<CP14WorkplaceComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CP14WorkplaceComponent, BeforeActivatableUIOpenEvent>(OnBeforeUIOpen);
    }

    private void OnPrototypeReload(PrototypesReloadedEventArgs ev)
    {
        if (!ev.WasModified<EntityPrototype>())
            return;

        var query = EntityQueryEnumerator<CP14WorkplaceComponent>();
        while (query.MoveNext(out var uid, out var workplace))
        {
            CacheWorkplaceRecipes((uid, workplace));
        }
    }

    private void OnMapInit(Entity<CP14WorkplaceComponent> ent, ref MapInitEvent args)
    {
        CacheWorkplaceRecipes(ent);
    }

    private void OnBeforeUIOpen(Entity<CP14WorkplaceComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        UpdateUIState(ent, args.User);
    }


    private void UpdateUIState(Entity<CP14WorkplaceComponent> entity, EntityUid user)
    {

    }

    private void CacheWorkplaceRecipes(Entity<CP14WorkplaceComponent> entity)
    {
        entity.Comp.CachedRecipes.Clear();

        var allEnts = _proto.EnumeratePrototypes<EntityPrototype>();

        foreach (var recipe in allEnts)
        {
            if (!recipe.Components.TryGetComponent(CP14WorkplaceRecipeComponent.CompName, out var compData) || compData is not CP14WorkplaceRecipeComponent recipeComp)
                continue;

            if (!entity.Comp.Tags.Contains(recipeComp.Tag))
                continue;

            entity.Comp.CachedRecipes.Add(recipe);
        }
    }
}
