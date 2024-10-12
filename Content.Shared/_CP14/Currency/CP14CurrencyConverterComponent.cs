using Content.Shared.Whitelist;

namespace Content.Shared._CP14.Currency;

/// <summary>
/// Reflects the market value of an item, to guide players through the economy.
/// </summary>

[RegisterComponent]
public sealed partial class CP14CurrencyConverterComponent : Component
{
    [DataField]
    public int Balance = 0;

    [DataField]
    public EntityWhitelist? Whitelist = null;
}
