using Content.Shared._CP14.Cooking.Components;
using Content.Shared.DoAfter;
using Content.Shared.Temperature;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Cooking;

public abstract partial class CP14SharedCookingSystem
{
    private readonly ProtoId<CP14CookingRecipePrototype> _burnedRecipe = "BurnedMeal"; //TODO add support to different meal types
    private void InitDoAfter()
    {
        SubscribeLocalEvent<CP14FoodCookerComponent, OnTemperatureChangeEvent>(OnTemperatureChange);
        SubscribeLocalEvent<CP14FoodCookerComponent, EntParentChangedMessage>(OnParentChanged);

        SubscribeLocalEvent<CP14FoodCookerComponent, CP14CookingDoAfter>(OnCookFinished);
        SubscribeLocalEvent<CP14FoodCookerComponent, CP14BurningDoAfter>(OnCookBurned);
    }

    private void StartCooking(Entity<CP14FoodCookerComponent> ent)
    {
        DoAfterArgs? doAfterArgs = null;
        if (ent.Comp.FoodData is null)
        {
            doAfterArgs = new DoAfterArgs(EntityManager, ent, 20, new CP14CookingDoAfter(), ent)
            {
                NeedHand = false,
                BreakOnWeightlessMove = false,
                RequireCanInteract = false,
            };
        }
        else
        {
            doAfterArgs = new DoAfterArgs(EntityManager, ent, 20, new CP14BurningDoAfter(), ent)
            {
                NeedHand = false,
                BreakOnWeightlessMove = false,
                RequireCanInteract = false,
            };
        }

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
        if (args.TemperatureDelta > 0)
        {
            if (ent.Comp.DoAfterId is null)
            {
                StartCooking(ent);
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

        var recipe = GetRecipe(ent);
        if (recipe is not null)
            CookFood(ent, recipe);

        StopCooking(ent);

        args.Handled = true;
    }

    private void OnCookBurned(Entity<CP14FoodCookerComponent> ent, ref CP14BurningDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        ForceCook(ent, _burnedRecipe);

        args.Handled = true;
    }

    private void ForceCook(Entity<CP14FoodCookerComponent> ent, ProtoId<CP14CookingRecipePrototype> forcedRecipe)
    {
        if (!_proto.TryIndex(forcedRecipe, out var indexedRecipe))
            return;

        CookFood(ent, indexedRecipe);
    }
}


[Serializable, NetSerializable]
public sealed partial class CP14CookingDoAfter : SimpleDoAfterEvent
{
}


[Serializable, NetSerializable]
public sealed partial class CP14BurningDoAfter : SimpleDoAfterEvent
{
}
