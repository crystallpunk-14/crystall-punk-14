using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Trading;

[Serializable, NetSerializable]
public enum CP14TradingUiKey
{
    Key,
}

[Serializable, NetSerializable]
public sealed class CP14TradingPlatformUiState(NetEntity user, TimeSpan nextTime) : BoundUserInterfaceState
{
    public NetEntity User = user;
    public TimeSpan NextBuyTime = nextTime;
}

[Serializable, NetSerializable]
public readonly struct CP14TradingProductEntry
{
}
