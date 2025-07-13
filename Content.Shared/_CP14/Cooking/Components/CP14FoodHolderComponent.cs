using Robust.Shared.GameStates;

namespace Content.Shared._CP14.Cooking.Components;

/// <summary>
///
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), Access(typeof(CP14SharedCookingSystem))]
public sealed partial class CP14FoodHolderComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<PrototypeLayerData>? Visuals = null;

    [DataField(required: true)]
    public CP14FoodType FoodType = default!;

    [DataField]
    public string? SolutionId;

    /// <summary>
    /// target layer, where new layers will be added. This allows you to control the order of generative layers and static layers.
    /// </summary>
    [DataField]
    public string TargetLayerMap = "cp14_foodLayers";

    public HashSet<string> RevealedLayers = new();
}
