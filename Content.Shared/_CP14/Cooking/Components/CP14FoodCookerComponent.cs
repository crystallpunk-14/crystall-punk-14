/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Cooking.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(CP14SharedCookingSystem))]
public sealed partial class CP14FoodCookerComponent : Component
{
    [DataField, AutoNetworkedField]
    public CP14FoodData? FoodData;

    [DataField(required: true)]
    public CP14FoodType FoodType;

    [DataField]
    public string ContainerId;

    [DataField]
    public string? SolutionId;

    public DoAfterId? DoAfterId = null;
}

[Serializable]
[DataDefinition]
public sealed partial class CP14FoodData
{
    [DataField]
    public ProtoId<CP14CookingRecipePrototype>? CurrentRecipe;

    [DataField]
    public LocId? Name;

    [DataField]
    public LocId? Desc;

    [DataField]
    public List<PrototypeLayerData> Visuals = new();

    [DataField]
    public List<EntProtoId> Trash = new();

    [DataField]
    public HashSet<LocId> Flavors = new();
}

public enum CP14FoodType
{
    Meal,
}
