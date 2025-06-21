using Content.Shared._CP14.Workbench;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Trading.Prototypes;

[Prototype("cp14TradingRequest")]
public sealed partial class CP14TradingRequestPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public HashSet<ProtoId<CP14TradingFactionPrototype>> PossibleFactions = [];

    [DataField]
    public float GenerationWeight = 1f;

    [DataField]
    public TimeSpan EarliestGenerationTime = TimeSpan.Zero;

    [DataField]
    public int AdditionalReward = 10;

    [DataField]
    public float ReputationReward = 0.25f;

    [DataField]
    public float RewardFluctuation = 0.6f;

    [DataField(required: true)]
    public List<CP14WorkbenchCraftRequirement> Requirements = new();
}
