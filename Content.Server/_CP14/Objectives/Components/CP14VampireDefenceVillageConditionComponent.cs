using Content.Server._CP14.Objectives.Systems;
using Robust.Shared.Utility;

namespace Content.Server._CP14.Objectives.Components;

[RegisterComponent, Access(typeof(CP14VampireObjectiveConditionsSystem))]
public sealed partial class CP14VampireDefenceVillageConditionComponent : Component
{
    [DataField]
    public float DefencePercentage = 0.5f;

    [DataField]
    public SpriteSpecifier Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_CP14/Actions/Spells/vampire.rsi"), "essence_create");
}
