using Content.Shared._CP14.Procedural.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.GameTicking.Rules.Components;

/// <summary>
/// A rule that assigns common goals to different roles. Common objectives are generated once at the beginning of a round and are shared between players.
/// </summary>
[RegisterComponent, Access(typeof(CP14ExpeditionToWindlandsRule))]
public sealed partial class CP14ExpeditionToWindlandsRuleComponent : Component
{
    [DataField]
    public ProtoId<CP14ProceduralLocationPrototype> Location = "T1GrasslandIsland";

    [DataField]
    public List<ProtoId<CP14ProceduralModifierPrototype>> Modifiers = [];

    [DataField]
    public float FloatingTime = 120;
}
