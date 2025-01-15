/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Server.Stack;
using Content.Shared._CP14.Knowledge;
using Content.Shared._CP14.Workbench;
using Content.Shared._CP14.Workbench.Prototypes;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.Stacks;
using Content.Shared.UserInterface;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Workbench;

public sealed partial class CP14WorkbenchSystem : SharedCP14WorkbenchSystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly StackSystem _stack = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedCP14KnowledgeSystem _knowledge = default!;

    private EntityQuery<MetaDataComponent> _metaQuery;
    private EntityQuery<StackComponent> _stackQuery;

    public override void Initialize()
    {
        base.Initialize();

        _metaQuery = GetEntityQuery<MetaDataComponent>();
        _stackQuery = GetEntityQuery<StackComponent>();

        SubscribeLocalEvent<CP14WorkbenchComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<CP14WorkbenchComponent, BeforeActivatableUIOpenEvent>(OnBeforeUIOpen);
        SubscribeLocalEvent<CP14WorkbenchComponent, CP14WorkbenchUiCraftMessage>(OnCraft);

        SubscribeLocalEvent<CP14WorkbenchComponent, CP14CraftDoAfterEvent>(OnCraftFinished);
    }

    private void OnMapInit(Entity<CP14WorkbenchComponent> ent, ref MapInitEvent args)
    {
        foreach (var recipe in _proto.EnumeratePrototypes<CP14WorkbenchRecipePrototype>())
        {
            if (ent.Comp.Recipes.Contains(recipe))
                continue;

            if (!ent.Comp.RecipeTags.Contains(recipe.Tag))
                continue;

            ent.Comp.Recipes.Add(recipe);
        }
    }

    private void OnBeforeUIOpen(Entity<CP14WorkbenchComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        UpdateUIRecipes(ent, args.User);
    }

    private void OnCraftFinished(Entity<CP14WorkbenchComponent> ent, ref CP14CraftDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!_proto.TryIndex(args.Recipe, out var recipe))
            return;

        var placedEntities = _lookup.GetEntitiesInRange(Transform(ent).Coordinates, ent.Comp.WorkbenchRadius, LookupFlags.Uncontained);

        if (!CanCraftRecipe(recipe, placedEntities, args.User))
        {
            _popup.PopupEntity(Loc.GetString("cp14-workbench-cant-craft"), ent, args.User);
            return;
        }

        if (!_proto.TryIndex(args.Recipe, out var indexedRecipe))
            return;

        if (recipe.KnowledgeRequired is not null)
            _knowledge.UseKnowledge(args.User, recipe.KnowledgeRequired.Value);

        var resultEntity = Spawn(indexedRecipe.Result);
        _stack.TryMergeToContacts(resultEntity);

        _solutionContainer.TryGetSolution(resultEntity, recipe.Solution, out var resultSolution, out _);
        if (recipe.TryMergeSolutions && resultSolution is not null)
        {
            resultSolution.Value.Comp.Solution.MaxVolume = 0;
            _solutionContainer.RemoveAllSolution(resultSolution.Value); //If we combine ingredient solutions, we do not use the default solution prescribed in the entity.
        }

        foreach (var requiredIngredient  in recipe.Entities)
        {
            var requiredCount = requiredIngredient.Value;
            foreach (var placedEntity in placedEntities)
            {
                if (!TryComp<MetaDataComponent>(placedEntity, out var metaData) || metaData.EntityPrototype is null)
                    continue;

                var placedProto = metaData.EntityPrototype.ID;
                if (placedProto == requiredIngredient.Key && requiredCount > 0)
                {
                    // Trying merge solutions
                    if (recipe.TryMergeSolutions
                        && resultSolution is not null
                        && _solutionContainer.TryGetSolution(placedEntity, recipe.Solution, out var ingredientSoln, out var ingredientSolution))
                    {
                        resultSolution.Value.Comp.Solution.MaxVolume += ingredientSoln.Value.Comp.Solution.MaxVolume;
                        _solutionContainer.TryAddSolution(resultSolution.Value, ingredientSolution);
                    }

                    requiredCount--;
                    Del(placedEntity);
                }
            }
        }

        foreach (var requiredStack in recipe.Stacks)
        {
            var requiredCount = requiredStack.Value;
            foreach (var placedEntity in placedEntities)
            {
                if (!_stackQuery.TryGetComponent(placedEntity, out var stack))
                    continue;

                if (stack.StackTypeId != requiredStack.Key)
                    continue;

                var count = (int)MathF.Min(requiredCount, stack.Count);

                if (stack.Count - count <= 0)
                    Del(placedEntity);
                else
                    _stack.SetCount(placedEntity, stack.Count - count, stack);

                requiredCount -= count;
            }
        }
        _transform.SetCoordinates(resultEntity,  Transform(ent).Coordinates);
        UpdateUIRecipes(ent, args.User);
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
        _audio.PlayPvs(recipe.OverrideCraftSound ?? workbench.Comp.CraftSound, workbench);
    }

    private bool CanCraftRecipe(CP14WorkbenchRecipePrototype recipe, HashSet<EntityUid> entities, EntityUid user)
    {
        //Knowledge check
        if (recipe.KnowledgeRequired is not null && !_knowledge.HasKnowledge(user, recipe.KnowledgeRequired.Value))
            return false;

        //Ingredients check
        var indexedIngredients = IndexIngredients(entities);
        foreach (var requiredIngredient  in recipe.Entities)
        {
            if (!indexedIngredients.TryGetValue(requiredIngredient.Key, out var availableQuantity) ||
                availableQuantity < requiredIngredient.Value)
                return false;
        }

        foreach (var (key, value) in recipe.Stacks)
        {
            var count = 0;
            foreach (var ent in entities)
            {
                if (_stackQuery.TryGetComponent(ent, out var stack))
                {
                    if (stack.StackTypeId != key)
                        continue;

                    count += stack.Count;
                }
            }

            if (count < value)
                return false;
        }
        return true;
    }

    private Dictionary<EntProtoId, int> IndexIngredients(HashSet<EntityUid> ingredients)
    {
        var indexedIngredients = new Dictionary<EntProtoId, int>();

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
