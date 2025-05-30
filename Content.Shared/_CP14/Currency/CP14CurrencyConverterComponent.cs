using System.Numerics;
using Content.Shared.Tag;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Currency;

/// <summary>
/// Reflects the market value of an item, to guide players through the economy.
/// </summary>

[RegisterComponent]
public sealed partial class CP14CurrencyConverterComponent : Component
{
    [DataField]
    public int Balance;

    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public Vector2 SpawnOffset = new Vector2(0, -0.4f);

    [DataField]
    public SoundSpecifier InsertSound = new SoundCollectionSpecifier("CP14Coins");

    [DataField]
    public ProtoId<TagPrototype> CoinTag = "CP14Coin";
}
