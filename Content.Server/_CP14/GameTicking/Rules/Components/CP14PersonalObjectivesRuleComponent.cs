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
}
