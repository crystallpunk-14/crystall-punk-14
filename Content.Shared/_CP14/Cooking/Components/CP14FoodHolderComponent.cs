/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CP14.Cooking.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Cooking.Components;

/// <summary>
/// Food of the specified type can be transferred to this entity.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), Access(typeof(CP14SharedCookingSystem))]
public sealed partial class CP14FoodHolderComponent : Component
{
    /// <summary>
    /// What food is currently stored here?
    /// </summary>
    [DataField, AutoNetworkedField]
    public CP14FoodData? FoodData;

    [DataField]
    public bool CanAcceptFood = false;

    [DataField]
    public bool CanGiveFood = false;

    [DataField(required: true)]
    public ProtoId<CP14FoodTypePrototype> FoodType;

    [DataField]
    public string? SolutionId;

    [DataField]
    public int MaxDisplacementFillLevels = 8;

    [DataField]
    public string? DisplacementRsiPath;

    /// <summary>
    /// target layer, where new layers will be added. This allows you to control the order of generative layers and static layers.
    /// </summary>
    [DataField]
    public string TargetLayerMap = "cp14_foodLayers";

    public HashSet<string> RevealedLayers = new();
}
