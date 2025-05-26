using System.Numerics;
using Content.Shared.FixedPoint;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Trading.Prototypes;

[Prototype("cp14TradingPosition")]
public sealed partial class CP14TradingPositionPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    /// <summary>
    /// Service Title. If you leave null, the name will try to generate from first Service.GetName()
    /// </summary>
    [DataField]
    public LocId? Name;

    /// <summary>
    /// Service Description. If you leave null, the description will try to generate from first Service.GetDescription()
    /// </summary>
    [DataField]
    public LocId? Desc;

    /// <summary>
    /// Icon for UI. If you leave null, the icon will try to generate from first Service.GetTexture()
    /// </summary>
    [DataField(required: true)]
    public SpriteSpecifier Icon = default!;

    [DataField(required: true)]
    public ProtoId<CP14TradingFactionPrototype> Faction;

    [DataField]
    public FixedPoint2 UnlockReputationCost = 1f;

    [DataField(required: true)]
    public Vector2 UiPosition = default!;

    [DataField(required: true)]
    public CP14StoreBuyService? Service = null;

    [DataField]
    public ProtoId<CP14TradingPositionPrototype>? Prerequisite;

    [DataField(required: true)]
    public int Price = 1;

    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(10);
}

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14StoreBuyService
{
    public abstract void Buy(EntityManager entManager, IPrototypeManager prototype,  EntityUid platform);

    public abstract string GetName(IPrototypeManager protoMan);

    public abstract string GetDesc(IPrototypeManager protoMan);
}
