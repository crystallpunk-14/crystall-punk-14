using Content.Shared._CP14.Cooking.Components;
using Content.Shared.DoAfter;
using Content.Shared.Temperature;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Cooking;

public abstract partial class CP14SharedCookingSystem
{
    private readonly ProtoId<CP14CookingRecipePrototype>
        _burnedRecipe = "BurnedMeal"; //TODO add support to different meal types

    private void InitDoAfter()
    {
        SubscribeLocalEvent<CP14FoodCookerComponent, OnTemperatureChangeEvent>(OnTemperatureChange);
        SubscribeLocalEvent<CP14FoodCookerComponent, EntParentChangedMessage>(OnParentChanged);

        SubscribeLocalEvent<CP14FoodCookerComponent, CP14CookingDoAfter>(OnCookFinished);
    }

    private void StartCooking(Entity<CP14FoodCookerComponent> ent, ProtoId<CP14CookingRecipePrototype> recipe)
    {
        if (!_proto.TryIndex(recipe, out var indexedRecipe))
            return;

        StartCooking(ent, indexedRecipe);
    }
    private void StartCooking(Entity<CP14FoodCookerComponent> ent, CP14CookingRecipePrototype recipe)
    {
        if (ent.Comp.DoAfterId is not null)
            return;

        var doAfterArgs = new DoAfterArgs(EntityManager, ent, recipe.CookingTime, new CP14CookingDoAfter(recipe.ID), ent)
        {
            NeedHand = false,
            BreakOnWeightlessMove = false,
            RequireCanInteract = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs, out var doAfterId);
        ent.Comp.DoAfterId = doAfterId;
        _ambientSound.SetAmbience(ent, true);
        _ambientSound.SetSound(ent, recipe.CookingAmbient);
    }

    private void StopCooking(Entity<CP14FoodCookerComponent> ent)
    {
        _doAfter.Cancel(ent.Comp.DoAfterId);
        ent.Comp.DoAfterId = null;
        _ambientSound.SetAmbience(ent, false);
    }

    private void OnTemperatureChange(Entity<CP14FoodCookerComponent> ent, ref OnTemperatureChangeEvent args)
    {
        if (args.TemperatureDelta > 0)
        {
            if (ent.Comp.DoAfterId is null)
            {
                var recipe = GetRecipe(ent);
                if (recipe is not null)
                    StartCooking(ent, recipe);
            }
            else
            {
                if (ent.Comp.FoodData?.CurrentRecipe != _burnedRecipe)
                    StartCooking(ent, _burnedRecipe);
            }
        }
        else
        {
            StopCooking(ent);
        }
    }

    private void OnParentChanged(Entity<CP14FoodCookerComponent> ent, ref EntParentChangedMessage args)
    {
        StopCooking(ent);
    }

    private void OnCookFinished(Entity<CP14FoodCookerComponent> ent, ref CP14CookingDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!_proto.TryIndex(args.Recipe, out var indexedRecipe))
            return;

        CookFood(ent, indexedRecipe);
        StopCooking(ent);

        args.Handled = true;
    }
}

[Serializable, NetSerializable]
public sealed partial class CP14CookingDoAfter : DoAfterEvent
{
    [DataField]
    public ProtoId<CP14CookingRecipePrototype> Recipe;

    public CP14CookingDoAfter(ProtoId<CP14CookingRecipePrototype> recipe)
    {
        Recipe = recipe;
    }

    public override DoAfterEvent Clone() => this;
}
