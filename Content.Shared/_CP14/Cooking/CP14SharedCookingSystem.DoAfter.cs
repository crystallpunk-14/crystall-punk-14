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

    private void StartBurning(Entity<CP14FoodCookerComponent> ent)
    {
        if (ent.Comp.DoAfterId is not null)
            return;

        var doAfterArgs = new DoAfterArgs(EntityManager, ent, 20, new CP14BurningDoAfter(), ent)
        {
            NeedHand = false,
            BreakOnWeightlessMove = false,
            RequireCanInteract = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs, out var doAfterId);
        ent.Comp.DoAfterId = doAfterId;
        _ambientSound.SetAmbience(ent, true);
        //_ambientSound.SetSound(ent, recipe.CookingAmbient); TODO
    }

    protected void StopCooking(Entity<CP14FoodCookerComponent> ent)
    {
        if (ent.Comp.DoAfterId is null)
            return;

        _doAfter.Cancel(ent.Comp.DoAfterId);
        ent.Comp.DoAfterId = null;
        _ambientSound.SetAmbience(ent, false);
    }

    private void OnTemperatureChange(Entity<CP14FoodCookerComponent> ent, ref OnTemperatureChangeEvent args)
    {
        if (!_container.TryGetContainer(ent, ent.Comp.ContainerId, out var container))
            return;

        if (container.ContainedEntities.Count <= 0 && ent.Comp.FoodData is null)
        {
            StopCooking(ent);
            return;
        }

        if (args.TemperatureDelta > 0)
        {
            if (ent.Comp.DoAfterId is null && ent.Comp.FoodData is null)
            {
                var recipe = GetRecipe(ent);
                if (recipe is not null)
                    StartCooking(ent, recipe);
            }
            else
            {
                StartBurning(ent);
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

[Serializable, NetSerializable]
public sealed partial class CP14BurningDoAfter : SimpleDoAfterEvent;
