using Content.Server._CP14.Objectives.Systems;
using Content.Shared._CP14.Vampire;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._CP14.Objectives.Components;

[RegisterComponent, Access(typeof(CP14VampireObjectiveConditionsSystem))]
public sealed partial class CP14VampireBloodPurityConditionComponent : Component
{
    [DataField]
    public ProtoId<CP14VampireFactionPrototype>? Faction;

    [DataField]
    public SpriteSpecifier Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_CP14/Actions/Spells/vampire.rsi"), "blood_moon");
}
