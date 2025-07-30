using Content.Shared.Random;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.GameTicking.Rules.Components;

/// <summary>
/// A rule that assigns common goals to different roles. Common objectives are generated once at the beginning of a round and are shared between players.
/// </summary>
[RegisterComponent, Access(typeof(CP14CrashToWindlandsRule))]
public sealed partial class CP14CrashingShipComponent : Component
{
    [DataField]
    public TimeSpan NextExplosionTime = TimeSpan.Zero;

    [DataField]
    public EntProtoId ExplosionProto = "CP14ShipExplosion";

    [DataField]
    public EntProtoId FinalExplosionProto = "CP14ShipExplosion";
}
