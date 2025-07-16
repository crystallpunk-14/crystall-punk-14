/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Cooking.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), Access(typeof(CP14SharedCookingSystem))]
public sealed partial class CP14FoodHolderComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<PrototypeLayerData>? Visuals;

    [DataField(required: true)]
    public CP14FoodType FoodType;

    [DataField]
    public string? SolutionId;

    [DataField]
    public int MaxDisplacementFillLevels = 8;

    [DataField]
    public string? DisplacementRsiPath = null;

    /// <summary>
    /// target layer, where new layers will be added. This allows you to control the order of generative layers and static layers.
    /// </summary>
    [DataField]
    public string TargetLayerMap = "cp14_foodLayers";

    public HashSet<string> RevealedLayers = new();
}
