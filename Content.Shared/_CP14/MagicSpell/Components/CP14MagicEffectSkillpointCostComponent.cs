using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Components;

/// <summary>
/// Restricts the use of this action, by spending user skillpoints
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem))]
public sealed partial class CP14MagicEffectSkillPointCostComponent : Component
{
    [DataField(required: true)]
    public ProtoId<CP14SkillPointPrototype>? SkillPoint;

    [DataField]
    public FixedPoint2 Count = 1f;
}
