using Content.Shared.Random;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.GameTicking.Rules.Components;

/// <summary>
/// A rule that assigns common goals to different roles. Common objectives are generated once at the beginning of a round and are shared between players.
/// </summary>
[RegisterComponent, Access(typeof(CP14PersonalObjectivesRule))]
public sealed partial class CP14CommonObjectivesRuleComponent : Component
{
    [DataField]
    public Dictionary<ProtoId<JobPrototype>, List<ProtoId<WeightedRandomPrototype>>> JobObjectives = new();

    [DataField]
    public Dictionary<ProtoId<DepartmentPrototype>, List<ProtoId<WeightedRandomPrototype>>> DepartmentObjectives = new();
}