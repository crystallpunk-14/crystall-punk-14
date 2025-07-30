using Content.Shared._CP14.Demiplane.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.GameTicking.Rules.Components;

/// <summary>
/// A rule that assigns common goals to different roles. Common objectives are generated once at the beginning of a round and are shared between players.
/// </summary>
[RegisterComponent, Access(typeof(CP14CrashToWindlandsRule))]
public sealed partial class CP14CrashToWindlandsRuleComponent : Component
{
    [DataField]
    public ProtoId<CP14DemiplaneLocationPrototype> Location = "T1GrasslandIsland";

    [DataField]
    public List<ProtoId<CP14DemiplaneModifierPrototype>> Modifiers = [];
}
