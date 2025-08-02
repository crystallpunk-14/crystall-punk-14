using Robust.Shared.Prototypes;

namespace Content.Server._CP14.GameTicking.Rules.Components;

/// <summary>
///When attached to shuttle, start firebombing it until FTL ends.
/// </summary>
[RegisterComponent, Access(typeof(CP14CrashingShipRule))]
public sealed partial class CP14CrashingShipComponent : Component
{
    [DataField]
    public TimeSpan NextExplosionTime = TimeSpan.Zero;

    [DataField]
    public EntProtoId ExplosionProto = "CP14ShipExplosion";

    [DataField]
    public EntProtoId FinalExplosionProto = "CP14ShipExplosionBig";
}
