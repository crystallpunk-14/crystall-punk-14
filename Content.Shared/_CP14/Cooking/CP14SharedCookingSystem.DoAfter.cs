using Content.Shared._CP14.Cooking.Components;
using Content.Shared.DoAfter;
using Content.Shared.Temperature;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Cooking;

public abstract partial class CP14SharedCookingSystem
{
    private readonly ProtoId<CP14CookingRecipePrototype> _burnedRecipe = "BurnedMeal"; //TODO add support to different meal types
    private void InitDoAfter()
    {
        SubscribeLocalEvent<CP14FoodCookerComponent, OnTemperatureChangeEvent>(OnTemperatureChange);
        SubscribeLocalEvent<CP14FoodCookerComponent, CP14CookingDoAfter>(OnCookFinished);
    }

    private void StartCooking(Entity<CP14FoodCookerComponent> ent)
    {
        var doAfterArgs = new DoAfterArgs(EntityManager, ent, 20, new CP14CookingDoAfter(), ent)
        {
            NeedHand = false,
            BreakOnWeightlessMove = false,
            RequireCanInteract = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs, out var doAfterId);
        ent.Comp.DoAfterId = doAfterId;
        _ambientSound.SetAmbience(ent, true);
    }

    private void StopCooking(Entity<CP14FoodCookerComponent> ent)
    {
        _doAfter.Cancel(ent.Comp.DoAfterId);
        ent.Comp.DoAfterId = null;
        _ambientSound.SetAmbience(ent, false);
    }

    private void OnTemperatureChange(Entity<CP14FoodCookerComponent> ent, ref OnTemperatureChangeEvent args)
    {
        if (ent.Comp.FoodData is not null)
            return;

        if (args.CurrentTemperature > ent.Comp.CookingTempThreshold && args.CurrentTemperature < ent.Comp.BurningTempThreshold)
        {
            if (ent.Comp.DoAfterId is null)
            {
                StartCooking(ent);
            }
            else
            {
                StopCooking(ent);
            }
        }

        if (args.CurrentTemperature >= ent.Comp.BurningTempThreshold)
        {
            ForceCook(ent, _burnedRecipe);
        }
    }

    private void OnCookFinished(Entity<CP14FoodCookerComponent> ent, ref CP14CookingDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        var recipe = GetRecipe(ent);
        if (recipe is not null)
            CookFood(ent, recipe);

        StopCooking(ent);

        args.Handled = true;
    }

    private void ForceCook(Entity<CP14FoodCookerComponent> ent, ProtoId<CP14CookingRecipePrototype> forcedRecipe)
    {
        if (!_proto.TryIndex(forcedRecipe, out var indexedRecipe))
            return;

        CookFood(ent, indexedRecipe);
    }
}
