namespace Content.Shared._CP14.Currency;

/// <summary>
/// Reflects the market value of an item, to guide players through the economy.
/// </summary>

[RegisterComponent, Access(typeof(CP14CurrencySystem))]
public sealed partial class CP14CurrencyComponent : Component
{
    [DataField]
    public int Currency = 1;

    /// <summary>
    /// allows you to categorize different valuable items in order to, for example, give goals for buying weapons, or earning money specifically.
    /// </summary>
    [DataField]
    public string? Category;
}
