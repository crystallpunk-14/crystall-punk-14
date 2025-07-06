using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Trading;

[Serializable, NetSerializable]
public enum CP14TradingUiKey
{
    Buy,
    Sell,
}

[Serializable, NetSerializable]
public sealed class CP14TradingPlatformUiState(NetEntity platform) : BoundUserInterfaceState
{
    public NetEntity Platform = platform;
}

[Serializable, NetSerializable]
public sealed class CP14SellingPlatformUiState(NetEntity platform, int price) : BoundUserInterfaceState
{
    public NetEntity Platform = platform;
    public int Price = price;
}

[Serializable, NetSerializable]
public readonly struct CP14TradingProductEntry
{
}
