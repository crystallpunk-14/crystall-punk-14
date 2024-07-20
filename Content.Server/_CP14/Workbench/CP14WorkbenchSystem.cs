using Content.Shared._CP14.Workbench;
using Content.Shared._CP14.Workbench.Prototypes;
using Content.Shared.Placeable;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Workbench;

public sealed class CP14WorkbenchSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private EntityQuery<MetaDataComponent> _metaQuery;
    public override void Initialize()
    {
        base.Initialize();

        _metaQuery = GetEntityQuery<MetaDataComponent>();

        SubscribeLocalEvent<CP14WorkbenchComponent, GetVerbsEvent<InteractionVerb>>(OnInteractionVerb);
    }

    private void OnInteractionVerb(Entity<CP14WorkbenchComponent> ent, ref GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands is null)
            return;

        if (!TryComp<ItemPlacerComponent>(ent, out var itemPlacer))
            return;

        var crafts = GetPossibleCrafts(ent, itemPlacer.PlacedEntities);
        var user = args.User;

        foreach (var craft in crafts)
        {
            args.Verbs.Add(new()
            {
                Act = () =>
                {
                    Spawn(craft.Result, Transform(ent).Coordinates);
                },
                Text = craft.Result,
                Category = VerbCategory.SelectType,
            });
        }
    }

    private List<CP14WorkbenchRecipePrototype> GetPossibleCrafts(Entity<CP14WorkbenchComponent> workbench, HashSet<EntityUid> ingrediEnts)
    {
        List<CP14WorkbenchRecipePrototype> result = new();

        if (ingrediEnts.Count == 0)
            return result;

        var indexedIngrediEnts = IndexIngredients(ingrediEnts);

        foreach (var recipeProto in workbench.Comp.Recipes)
        {
            var recipe = _proto.Index(recipeProto);

            if (CanCraftRecipe(recipe, indexedIngrediEnts))
            {
                result.Add(recipe);
            }
        }

        return result;
    }

    private bool CanCraftRecipe(CP14WorkbenchRecipePrototype recipe, Dictionary<string, int> indexedIngredients)
    {
        foreach (var requiredIngredient  in recipe.Entities)
        {
            if (!indexedIngredients.TryGetValue(requiredIngredient.Key, out int avalaibleQuantity) ||
                avalaibleQuantity < requiredIngredient.Value)
                return false;
        }

        return true;
    }

    private Dictionary<string, int> IndexIngredients(HashSet<EntityUid> ingredients)
    {
        var indexedIngredients = new Dictionary<string, int>();

        foreach (var ingredient in ingredients)
        {
            var protoId = _metaQuery.GetComponent(ingredient).EntityPrototype?.ID;
            if (protoId == null)
                continue;

            if (indexedIngredients.ContainsKey(protoId))
                indexedIngredients[protoId]++;
            else
                indexedIngredients[protoId] = 1;
        }
        return indexedIngredients;
    }
}
