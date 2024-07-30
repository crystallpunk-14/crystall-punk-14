using Content.Server.GameTicking.Rules;
using Content.Shared.Random;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.GameTicking.Rules.Components;

/// <summary>
/// A rule issuing a common goal for all expedition participants
/// </summary>
[RegisterComponent, Access(typeof(CP14ExpeditionObjectivesRule))]
public sealed partial class CP14ExpeditionObjectivesRuleComponent : Component
{
    [DataField]
    public Dictionary<ProtoId<JobPrototype>, List<ProtoId<WeightedRandomPrototype>>> RoleObjectives = new();

    [DataField]
    public Dictionary<ProtoId<DepartmentPrototype>, List<ProtoId<WeightedRandomPrototype>>> DepartmentObjectives = new();
}
