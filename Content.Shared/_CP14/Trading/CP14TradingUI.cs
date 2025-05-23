using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Trading;

[Serializable, NetSerializable]
public enum CP14TradingUiKey
{
    Key,
}

[Serializable, NetSerializable]
public sealed class CP14TradingPlatformUiState : BoundUserInterfaceState
{
    public CP14TradingPlatformUiState()
    {
    }
}

[Serializable, NetSerializable]
public readonly struct CP14TradingProductEntry
{
}
