/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Cooking.Prototypes;

[Prototype("CP14FoodType")]
public sealed class CP14FoodTypePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;
}
