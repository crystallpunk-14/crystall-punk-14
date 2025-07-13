using Content.Shared._CP14.Cooking.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Cooking;

[Prototype("CP14CookingRecipe")]
public sealed class CP14CookingRecipePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Mandatory conditions, without which the craft button will not even be active
    /// </summary>
    [DataField(required: true)]
    public List<CP14CookingCraftRequirement> Requirements = new();

    [DataField]
    public CP14FoodData FoodData = new();

    [DataField]
    public CP14FoodType FoodType = CP14FoodType.Meal;
}
