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
public sealed class CP14TradingPlatformUiState(NetEntity user) : BoundUserInterfaceState
{
    public NetEntity User = user;
}

[Serializable, NetSerializable]
public readonly struct CP14TradingProductEntry
{
}
