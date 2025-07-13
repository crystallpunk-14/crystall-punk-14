using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Cooking.Components;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CP14CookingSystem))]
[EntityCategory("HideSpawnMenu")]
public sealed partial class CP14FoodRecipeComponent : Component
{
    [DataField(required: true)]
    public CP14FoodType FoodType = default!;

    [DataField]
    public CP14FoodData FoodData = new();

    [DataField]
    public List<CP14CookingCraftRequirement> Conditions = new();

    [DataField]
    List<PrototypeLayerData> Visuals = new();
}

public enum CP14FoodType
{
    Soup,
    Meal,
    Kebab,
    Alcohol,
}
