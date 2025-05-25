using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Trading;

[Serializable, NetSerializable]
public enum CP14TradingUiKey
{
    Key,
}

[Serializable, NetSerializable]
public sealed class CP14TradingPlatformUiState(NetEntity user, NetEntity platform, TimeSpan nextTime) : BoundUserInterfaceState
{
    public NetEntity User = user;
    public NetEntity Platform = platform;
    public TimeSpan NextBuyTime = nextTime;
}

[Serializable, NetSerializable]
public readonly struct CP14TradingProductEntry
{
}
