using Content.Shared._CP14.Trading.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Trading;

[Serializable, NetSerializable]
public enum CP14TradingUiKey
{
    Key,
}

[Serializable, NetSerializable]
public sealed class CP14TradingPlatformUiState(
    Dictionary<ProtoId<CP14TradingFactionPrototype>, float> reputation,
    HashSet<ProtoId<CP14TradingPositionPrototype>> unlockedPositions
) : BoundUserInterfaceState
{
    public Dictionary<ProtoId<CP14TradingFactionPrototype>, float> Reputation = reputation;
    public HashSet<ProtoId<CP14TradingPositionPrototype>> UnlockedPositions = unlockedPositions;
}

[Serializable, NetSerializable]
public readonly struct CP14TradingProductEntry
{
}
