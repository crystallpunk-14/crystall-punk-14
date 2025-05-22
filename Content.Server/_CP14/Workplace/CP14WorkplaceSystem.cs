using Content.Shared._CP14.Workplace;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Workplace;

public sealed partial class CP14WorkplaceSystem : CP14SharedWorkbenchSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;

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
        var recipes = new List<CP14WorkplaceRecipeEntry>();
        foreach (var recipe in entity.Comp.CachedRecipes)
        {
            var entry = new CP14WorkplaceRecipeEntry(recipe, true);
            recipes.Add(entry);
        }
        _userInterface.SetUiState(entity.Owner, CP14WorkplaceUiKey.Key, new CP14WorkplaceState(recipes));
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
