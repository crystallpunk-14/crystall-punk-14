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

    /// <summary>
    /// a unique code that can be used to purchase items.
    /// </summary>
    [DataField(required: true)]
    public string Code = string.Empty;

    [DataField(required: true, serverOnly: true)]
    public CP14StoreBuyService Service = default!;

    [DataField]
    public SpriteSpecifier? IconOverride;

    [DataField]
    public LocId? NameOverride;

    [DataField]
    public bool RoundstartAvailable = true;

    /// <summary>
    /// If true, this item will randomly appear under the ‘Special Offer’ heading. With a chance to show up every time the ship arrives.
    /// </summary>
    [DataField]
    public bool Special;

    [DataField]
    public HashSet<ProtoId<CP14StoreFactionPrototype>> Factions = new();
}

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14StoreBuyService
{
    public abstract void Buy(EntityManager entManager, IPrototypeManager prototype,  Entity<CP14TradingPortalComponent> portal);

    public abstract string GetName(IPrototypeManager protoMan);

    /// <summary>
    /// You can specify an icon generated from an entity. It will support layering, colour changes and other layer options. Return null to disable.
    /// </summary>
    public abstract EntProtoId? GetEntityView(IPrototypeManager protoManager);

    /// <summary>
    /// You can specify the texture directly. Return null to disable.
    /// </summary>
    public abstract SpriteSpecifier? GetTexture(IPrototypeManager protoManager);
}
