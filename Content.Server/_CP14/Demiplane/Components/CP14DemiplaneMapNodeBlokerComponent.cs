using Content.Server._CP14.DemiplaneTraveling;

namespace Content.Server._CP14.Demiplane.Components;

/// <summary>
/// The existence of an entity with this component will block the discovery of a particular coordinate in the demiplane navigation map.
/// </summary>
[RegisterComponent, Access(typeof(CP14StationDemiplaneMapSystem))]
public sealed partial class CP14DemiplaneMapNodeBlockerComponent : Component
{
    [DataField]
    public EntityUid? Station = null;

    [DataField]
    public Vector2i Position = new (0,0);
}
