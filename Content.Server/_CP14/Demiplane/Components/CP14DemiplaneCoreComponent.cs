using Content.Server._CP14.DemiplaneTraveling;

namespace Content.Server._CP14.Demiplane.Components;

[RegisterComponent, Access(typeof(CP14DemiplaneSystem), typeof(CP14StationDemiplaneMapSystem))]
public sealed partial class CP14DemiplaneCoreComponent : Component
{
    [DataField]
    public EntityUid? Demiplane;

    [DataField]
    public EntityUid? Station;

    [DataField]
    public Vector2i Position;
}
