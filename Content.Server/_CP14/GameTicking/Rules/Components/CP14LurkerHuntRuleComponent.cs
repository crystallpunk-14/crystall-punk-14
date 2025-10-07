using Robust.Shared.Prototypes;

namespace Content.Server._CP14.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(CP14LurkerHuntRule))]
public sealed partial class CP14LurkerHuntRuleComponent : Component
{
    [DataField]
    public EntProtoId LurkerProto = "CP14MobLurker";

    [DataField]
    public EntityUid? Lurker;

    [DataField]
    public HashSet<EntityUid> Hunters = new();

    [DataField]
    public HashSet<EntityUid> Victims = new();
}
