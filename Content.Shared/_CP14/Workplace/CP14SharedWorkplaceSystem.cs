using Content.Shared.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Workplace;

public abstract partial class CP14SharedWorkplaceSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    /// <summary>
    /// All recipes are stored here in the dictionary.
    /// These are by design readonly entities for which events are called to collect information on them.
    /// </summary>
    private Dictionary<EntProtoId, Entity<CP14WorkplaceRecipeComponent>> _cachedRecipes = new();

    public override void Initialize()
    {
        base.Initialize();

        CacheAllRecipes();

        SubscribeLocalEvent<CP14WorkplaceComponent, BeforeActivatableUIOpenEvent>(OnBeforeUIOpen);
        SubscribeLocalEvent<CP14WorkplaceComponent, CP14WorkplaceCraftMessage>(OnCraftAttempt);

        SubscribeLocalEvent<CP14WorkplaceComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypeReload);
    }

    private void OnBeforeUIOpen(Entity<CP14WorkplaceComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        UpdateUIState(ent, args.User);
    }

    private void OnCraftAttempt(Entity<CP14WorkplaceComponent> ent, ref CP14WorkplaceCraftMessage args)
    {
        if (!_cachedRecipes.TryGetValue(args.Recipe, out var cachedRecipe))
            return;

        if (!ent.Comp.CachedRecipes.Contains(cachedRecipe))
            return;
    }

    private void OnMapInit(Entity<CP14WorkplaceComponent> ent, ref MapInitEvent args)
    {
        CacheWorkplaceRecipes(ent);
    }

    private void OnPrototypeReload(PrototypesReloadedEventArgs ev)
    {
        if (!ev.WasModified<EntityPrototype>())
            return;

        CacheAllRecipes();

        var query = EntityQueryEnumerator<CP14WorkplaceComponent>();
        while (query.MoveNext(out var uid, out var workplace))
        {
            CacheWorkplaceRecipes((uid, workplace));
        }
    }

    private void UpdateUIState(Entity<CP14WorkplaceComponent> entity, EntityUid user)
    {
        var recipes = new List<CP14WorkplaceRecipeEntry>();
        foreach (var recipe in entity.Comp.CachedRecipes)
        {
            var proto = MetaData(recipe).EntityPrototype;
            if (proto is null)
                continue;
            var entry = new CP14WorkplaceRecipeEntry(proto);
            recipes.Add(entry);
        }
        _userInterface.SetUiState(entity.Owner, CP14WorkplaceUiKey.Key, new CP14WorkplaceState(GetNetEntity(user), GetNetEntity(entity), recipes));
    }

    public bool CheckCraftable(EntProtoId recipe, EntityUid? workplace, EntityUid? user)
    {
        if (!TryComp<CP14WorkplaceComponent>(workplace, out var workplaceComp))
            return false;

        if (!_cachedRecipes.TryGetValue(recipe, out var cachedRecipe))
            return false;

        return CheckCraftable(cachedRecipe, workplace, user);
    }

    public bool CheckCraftable(EntityUid recipe, EntityUid? workplace, EntityUid? user)
    {
        if (user is null || workplace is null)
            return false;

        var ev = new CP14WorkplaceRequirementsPass(user.Value, workplace.Value, recipe);
        RaiseLocalEvent(recipe, ev);

        if (ev.Cancelled)
            return false;

        return true;
    }

    private void CacheAllRecipes()
    {
        //Delete all old cached recipes entity
        foreach (var recipe in _cachedRecipes.Values)
        {
            QueueDel(recipe);
        }

        var allEnts = _proto.EnumeratePrototypes<EntityPrototype>();

        foreach (var recipe in allEnts)
        {
            if (!recipe.Components.TryGetComponent(CP14WorkplaceRecipeComponent.CompName, out var compData) || compData is not CP14WorkplaceRecipeComponent recipeComp)
                continue;

            if (_cachedRecipes.ContainsKey(recipe.ID))
                continue;

            var ent = Spawn(recipe.ID);
            var entComp = EnsureComp<CP14WorkplaceRecipeComponent>(ent);

            _cachedRecipes.Add(recipe.ID, (ent, entComp));
        }
    }

    private void CacheWorkplaceRecipes(Entity<CP14WorkplaceComponent> entity)
    {
        entity.Comp.CachedRecipes.Clear();

        foreach (var recipe in _cachedRecipes.Values)
        {
            if (!entity.Comp.Tags.Contains(recipe.Comp.Tag))
                continue;

            entity.Comp.CachedRecipes.Add(recipe);
        }
    }
}

public sealed class CP14WorkplaceRequirementsPass(EntityUid user, EntityUid workplace, EntityUid recipe)
    : CancellableEntityEventArgs
{
    public EntityUid User { get; } = user;
    public EntityUid Workplace { get; } = workplace;
    public EntityUid Recipe { get; } = recipe;
}
