using Content.Server.GameTicking.Rules;

namespace Content.Server._CP14.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(CP14MapSpecificRuleSystem))]
public sealed partial class CP14MapSpecificRuleComponent : Component
{
    /// <summary>
    /// The gamerules that get added by map specific rule.
    /// </summary>
    [DataField]
    public HashSet<EntityUid> AdditionalGameRules = new();
}
