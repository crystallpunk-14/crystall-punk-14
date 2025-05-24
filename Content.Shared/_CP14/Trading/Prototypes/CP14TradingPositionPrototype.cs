using System.Numerics;
using Content.Shared._CP14.Cargo.Prototype;
using Content.Shared._CP14.Skill.Restrictions;
using Content.Shared.FixedPoint;
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
    public LocId? Name = null;

    /// <summary>
    /// Service Description. If you leave null, the description will try to generate from first Service.GetDescription()
    /// </summary>
    [DataField]
    public LocId? Desc = null;

    [DataField(required: true)]
    public ProtoId<CP14TradingFactionPrototype> Faction;

    [DataField]
    public FixedPoint2 UnlockReputationCost = 1f;

    [DataField]
    public int SponsorCost = 1;

    [DataField(required: true)]
    public Vector2 UiPosition = default!;

    [DataField(required: true)]
    public SpriteSpecifier Icon = default!;

    [DataField]
    public List<CP14StoreBuyService> Services = new();

    [DataField]
    public ProtoId<CP14TradingPositionPrototype>? Prerequisite;
}
