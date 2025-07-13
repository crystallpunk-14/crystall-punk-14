using Content.Shared.Chemistry.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Cooking.Components;

/// <summary>
///
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), Access(typeof(CP14CookingSystem))]
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
}

/// <summary>
///
/// </summary>
[Serializable]
[DataDefinition]
public sealed partial class CP14FoodData
{
    [DataField]
    public List<PrototypeLayerData> Visuals = new();

    [DataField]
    public List<EntProtoId> Trash = new();

    [DataField]
    public HashSet<LocId> Flavors = new();

    [DataField]
    public string? Name = null;

    [DataField]
    public LocId? Desc = null;

    [DataField]
    public Solution Solution = new();
}
