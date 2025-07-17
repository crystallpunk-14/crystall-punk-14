/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CP14.Cooking.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Cooking.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(CP14SharedCookingSystem))]
public sealed partial class CP14FoodHolderComponent : Component
{
    [DataField]
    public bool HoldFood = false;

    [DataField(required: true)]
    public ProtoId<CP14FoodTypePrototype> FoodType;

    [DataField]
    public string? SolutionId;
}
