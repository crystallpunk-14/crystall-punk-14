using Content.Shared.Mind;
using Content.Shared.Random;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.GameTicking.Rules.Components;

/// <summary>
/// a rule that assigns individual goals to different roles
/// </summary>
[RegisterComponent, Access(typeof(CP14PersonalObjectivesRule))]
public sealed partial class CP14PersonalObjectivesRuleComponent : Component
{
    [DataField]
    public Dictionary<ProtoId<JobPrototype>, List<ProtoId<WeightedRandomPrototype>>> RoleObjectives = new();

    [DataField]
    public Dictionary<ProtoId<DepartmentPrototype>, List<ProtoId<WeightedRandomPrototype>>> DepartmentObjectives = new();

    /// <summary>
    /// All of the objectives added by this rule. 1 mind -> many objectives
    /// </summary>
    [DataField]
    public Dictionary<EntityUid, List<EntityUid>> PersonalObjectives = new();
}
