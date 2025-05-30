using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Trading;

/// <summary>
/// Allows you to sell items by overloading the platform with energy
/// </summary>
[RegisterComponent]
public sealed partial class CP14SellingPlatformComponent : Component
{
    [DataField]
    public SoundSpecifier SellSound = new SoundPathSpecifier("/Audio/_CP14/Effects/cash.ogg")
    {
        Params = AudioParams.Default.WithVariation(0.1f),
    };

    [DataField]
    public EntProtoId SellVisual = "CP14CashImpact";
}
