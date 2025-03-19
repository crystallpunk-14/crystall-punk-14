using Content.Server._CP14.GameTicking.Rules;
using Content.Shared.Body.Prototypes;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Vampire;

[RegisterComponent]
[Access(typeof(CP14VampireRuleSystem))]
public sealed partial class CP14VampireComponent : Component
{
    [DataField]
    public ProtoId<ReagentPrototype> NewBloodReagent = "CP14BloodVampire";

    [DataField]
    public ProtoId<MetabolizerTypePrototype> MetabolizerType = "CP14Vampire";

    [DataField]
    public float HeatUnderSunTemperature = 12000f;

    [DataField]
    public TimeSpan HeatFrequency = TimeSpan.FromSeconds(1);

    [DataField]
    public TimeSpan NextHeatTime = TimeSpan.Zero;

    [DataField]
    public float IgniteThreshold = 350f;
}
