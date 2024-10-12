namespace Content.Shared._CP14.Currency;

/// <summary>
/// Reflects the market value of an item, to guide players through the economy.
/// </summary>

[RegisterComponent, Access(typeof(CP14SharedCurrencySystem))]
public sealed partial class CP14CurrencyComponent : Component
{
    [DataField]
    public int Currency = 1;
}
