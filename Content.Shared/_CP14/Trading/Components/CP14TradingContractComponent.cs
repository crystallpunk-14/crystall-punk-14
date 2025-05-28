using Content.Shared._CP14.Trading.Prototypes;
using Content.Shared._CP14.Trading.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Trading.Components;

[RegisterComponent, Access(typeof(CP14SharedTradingPlatformSystem))]
public sealed partial class CP14TradingContractComponent : Component
{
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(2);

    [DataField]
    public ProtoId<CP14TradingFactionPrototype> Faction;

    [DataField]
    public float StartReputation = 1f;
}
