using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Trading;

[Serializable, NetSerializable]
public enum CP14TradingUiKey
{
    Key,
}

[Serializable, NetSerializable]
public sealed class CP14TradingPlatformUiState(NetEntity platform) : BoundUserInterfaceState
{
    public NetEntity Platform = platform;
}

[Serializable, NetSerializable]
public readonly struct CP14TradingProductEntry
{
}
