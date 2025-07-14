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
    public CP14FoodType FoodType = default!;

    [DataField]
    public string ContainerId;

    [DataField]
    public string? SolutionId;

    [DataField]
    public float CookingTempThreshold = 600.00f;

    [DataField]
    public float BurningTempThreshold = 800.00f;

    public DoAfterId? DoAfterId = null;
}

[Serializable]
[DataDefinition]
public sealed partial class CP14FoodData
{
    [DataField]
    public LocId? Name = null;

    [DataField]
    public LocId? Desc = null;

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
