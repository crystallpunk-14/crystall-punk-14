using Content.Server._CP14.GameTicking.Rules;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Vampire;

[RegisterComponent, AutoGenerateComponentState]
[Access(typeof(CP14VampireRuleSystem))]
public sealed partial class CP14VampireComponent : Component
{
    [DataField]
    public ProtoId<ReagentPrototype> NewBloodReagent = "CP14BloodVampire";
}
