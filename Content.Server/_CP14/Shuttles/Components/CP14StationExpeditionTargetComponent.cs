using Robust.Shared.Utility;

namespace Content.Server._CP14.Shuttles.Components;

/// <summary>
/// Added to a station to start the round with an elemental ship arriving on this map
/// </summary>
[RegisterComponent, Access(typeof(CP14ExpeditionSystem))]
public sealed partial class CP14StationExpeditionTargetComponent : Component
{
    [DataField]
    public EntityUid Shuttle;

    [DataField] public ResPath ShuttlePath = new("/Maps/Shuttles/arrivals.yml");
}
