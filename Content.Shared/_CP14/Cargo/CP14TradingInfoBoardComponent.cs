using Content.Shared._CP14.Cargo.Prototype;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Cargo;

/// <summary>
/// Allows users to view information on faction trading opportunities
/// </summary>
[RegisterComponent]
public sealed partial class CP14TradingInfoBoardComponent : Component
{
    [DataField]
    public EntityUid? TradingPortal = null;

    [DataField]
    public ProtoId<CP14StoreFactionPrototype>? CahcedFaction;
}
