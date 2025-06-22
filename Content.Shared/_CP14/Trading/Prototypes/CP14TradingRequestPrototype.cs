using Content.Shared._CP14.Workbench;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Trading.Prototypes;

[Prototype("cp14TradingRequest")]
public sealed partial class CP14TradingRequestPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField]
    public bool AllFactions = false;

    [DataField]
    public HashSet<ProtoId<CP14TradingFactionPrototype>> PossibleFactions = [];

    [DataField]
    public float GenerationWeight = 1f;

    [DataField]
    public int FromMinutes = 0;

    [DataField]
    public int? ToMinutes;

    [DataField]
    public int AdditionalReward = 10;

    [DataField]
    public float ReputationCashback = 0.015f;

    [DataField(required: true)]
    public List<CP14WorkbenchCraftRequirement> Requirements = new();
}
