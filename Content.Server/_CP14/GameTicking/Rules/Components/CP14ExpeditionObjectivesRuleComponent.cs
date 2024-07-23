using Content.Server.GameTicking.Rules;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.GameTicking.Rules.Components;

/// <summary>
/// A rule issuing a common goal for all expedition participants
/// </summary>
[RegisterComponent, Access(typeof(CP14ExpeditionObjectivesRule))]
public sealed partial class CP14ExpeditionObjectivesRuleComponent : Component
{
    [DataField]
    public List<EntProtoId> Objectives = new();
}
