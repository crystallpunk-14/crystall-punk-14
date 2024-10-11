using Content.Shared.Destructible.Thresholds;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.TravelingStoreShip.Prototype;

/// <summary>
/// Stores the price and product/service pair that players can buy.
/// </summary>
[Prototype("storePositionBuy")]
public sealed partial class CP14StoreBuyPositionPrototype : IPrototype
{
    [IdDataField, ViewVariables]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public MinMax Price = new();

    [DataField(required: true)]
    public LocId Title = string.Empty;

    [DataField(required: true)]
    public SpriteSpecifier Icon = default!;

    [DataField(required: true)]
    public List<CP14StoreBuyService> Services = new();
}

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14StoreBuyService
{
    public abstract void Buy(EntityManager entManager, EntityUid station);

    public abstract string? GetDescription(IPrototypeManager prototype, IEntityManager entSys);
}
