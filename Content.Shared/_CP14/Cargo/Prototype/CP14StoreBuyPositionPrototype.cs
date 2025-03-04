using Content.Shared.Destructible.Thresholds;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Cargo.Prototype;

/// <summary>
/// Stores the price and product/service pair that players can buy.
/// </summary>
[Prototype("storePositionBuy")]
public sealed partial class CP14StoreBuyPositionPrototype : IPrototype
{
    [IdDataField, ViewVariables]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public int Price = 100;

    [DataField(required: true)]
    public LocId Name = string.Empty;

    /// <summary>
    /// a unique code that can be used to purchase items.
    /// </summary>
    [DataField(required: true)]
    public string Code = string.Empty;

    [DataField]
    public LocId Desc = string.Empty;

    [DataField(required: true)]
    public SpriteSpecifier Icon = default!;

    [DataField(required: true)]
    public List<CP14StoreBuyService> Services = new();

    [DataField]
    public bool RoundstartAvailable = true;

    /// <summary>
    /// If true, this item will randomly appear under the ‘Special Offer’ heading. With a chance to show up every time the ship arrives.
    /// </summary>
    [DataField]
    public bool Special = false;
}

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14StoreBuyService
{
    public abstract void Buy(EntityManager entManager, IPrototypeManager prototype,  Entity<CP14TradingPortalComponent> portal);
}
