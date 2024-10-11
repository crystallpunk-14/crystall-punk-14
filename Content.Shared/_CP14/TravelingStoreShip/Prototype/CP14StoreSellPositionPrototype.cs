using Content.Shared.Destructible.Thresholds;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.TravelingStoreShip;

/// <summary>
/// Stores the price and product/service pair that players can buy.
/// </summary>
[Prototype("storePositionSell")]
public sealed partial class CP14StoreSellPositionPrototype : IPrototype
{
    [IdDataField, ViewVariables]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public MinMax Price = new();

    [DataField(required: true)]
    public LocId Title = string.Empty;

    [DataField]
    public LocId FlavorDesc = string.Empty;

    [DataField(required: true)]
    public SpriteSpecifier Icon = default!;

    [DataField(required: true)]
    public CP14StoreSellService Service = default!;
}

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14StoreSellService
{
    public abstract bool TrySell(EntityManager entManager, HashSet<EntityUid> entities);

    public abstract string? GetDescription(IPrototypeManager prototype, IEntityManager entSys);
}
