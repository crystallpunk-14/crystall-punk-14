using Robust.Shared.Audio;

namespace Content.Shared._CP14.DemiplaneTraveling;

/// <summary>
/// teleports a certain number of entities to coordinate with a delay
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class CP14MonolithTimedPasswayComponent : Component
{
    [DataField]
    public int MaxEntities = 3;

    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(10f);

    [DataField]
    public float Radius = 3f;

    [DataField]
    [AutoPausedField]
    public TimeSpan NextTimeTeleport = TimeSpan.Zero;

    [DataField("arrivalSound")]
    public SoundSpecifier ArrivalSound = new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg");

    [DataField("departureSound")]
    public SoundSpecifier DepartureSound = new SoundPathSpecifier("/Audio/Effects/teleport_departure.ogg");
}
