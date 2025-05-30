using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Trading.Prototypes;

[Prototype("cp14TradingFaction")]
public sealed partial class CP14TradingFactionPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Name = default!;

    [DataField]
    public Color Color = Color.White;

    /// <summary>
    /// If not null, this faction is automatically unlocked for players, and grants the specified amount of reputation to unlock the first positions.
    /// </summary>
    [DataField]
    public float? RoundStart = null;
}
