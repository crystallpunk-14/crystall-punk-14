using Content.Shared._CP14.Trading.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._CP14.Trading.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(CP14SharedTradingPlatformSystem))]
public sealed partial class CP14TradingPlatformComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan NextBuyTime = TimeSpan.Zero;

    [DataField]
    public SoundSpecifier BuySound = new SoundPathSpecifier("/Audio/_CP14/Effects/cash.ogg")
    {
        Params = AudioParams.Default.WithVariation(0.1f)
    };
}
