using System.Linq;
using Content.Server.Popups;
using Content.Shared._CP14.Workbench;
using Content.Shared._CP14.Workbench.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Placeable;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Workbench;

public sealed class CP14WorkbenchSystem : SharedCP14WorkbenchSystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    private EntityQuery<MetaDataComponent> _metaQuery;
    public override void Initialize()
    {
        base.Initialize();

        _metaQuery = GetEntityQuery<MetaDataComponent>();

        SubscribeLocalEvent<CP14WorkbenchComponent, GetVerbsEvent<InteractionVerb>>(OnInteractionVerb);
        SubscribeLocalEvent<CP14WorkbenchComponent, CP14CraftDoAfterEvent>(OnCraftFinished);
    }

    private void OnInteractionVerb(Entity<CP14WorkbenchComponent> ent, ref GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands is null)
            return;

        if (!TryComp<ItemPlacerComponent>(ent, out var itemPlacer))
            return;

        var user = args.User;
        var crafts = GetPossibleCrafts(ent, itemPlacer.PlacedEntities);
        foreach (var craft in crafts)
        {
            args.Verbs.Add(new()
            {
                Act = () =>
                {
                    StartCraft(ent, user, craft);
                },
                Text = craft.Result,
                Category = VerbCategory.SelectType,
            });
        }
    }

    private void OnCraftFinished(Entity<CP14WorkbenchComponent> ent, ref CP14CraftDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        var recipe = _proto.Index(args.Recipe);
        if (!TryComp<ItemPlacerComponent>(ent, out var itemPlacer))
        {
            _popup.PopupEntity(Loc.GetString("cp14-workbench-no-resource"), ent, args.User);
            return;
        }

        var ingredientsOnWorkbench = IndexIngredients(itemPlacer.PlacedEntities);
        if (!CanCraftRecipe(recipe, ingredientsOnWorkbench))
        {
            _popup.PopupEntity(Loc.GetString("cp14-workbench-no-resource"), ent, args.User);
            return;
        }

        foreach (var requiredIngredient  in recipe.Entities)
        {
            int requiredCount = requiredIngredient.Value;
            foreach (var placedEntity in itemPlacer.PlacedEntities)
            {
                var placedProto = MetaData(placedEntity).EntityPrototype?.ID;
                if (placedProto != null && placedProto == requiredIngredient.Key && requiredCount > 0)
                {
                    requiredCount--;
                    QueueDel(placedEntity);
                }
            }
        }

        Spawn(_proto.Index(args.Recipe).Result, Transform(ent).Coordinates);

        args.Handled = true;
    }

    private void StartCraft(Entity<CP14WorkbenchComponent> workbench, EntityUid user, CP14WorkbenchRecipePrototype recipe)
    {
        var craftDoAfter = new CP14CraftDoAfterEvent
        {
            Recipe = recipe.ID,
        };

        var doAfterArgs = new DoAfterArgs(EntityManager,
            user,
            recipe.CraftTime * workbench.Comp.CraftSpeed,
            craftDoAfter,
            workbench,
            workbench)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
        _audio.PlayPvs(recipe.CraftSound, workbench);
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
