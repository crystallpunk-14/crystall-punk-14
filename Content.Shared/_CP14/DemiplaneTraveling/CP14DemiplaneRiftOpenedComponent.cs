using Robust.Shared.Audio;

namespace Content.Shared._CP14.DemiplaneTraveling;

/// <summary>
/// if located on an entity with a CP14DemiplanRiftComponent, allows users to move through that rift via an interaction with doAfter
/// </summary>
[RegisterComponent]
public sealed partial class CP14DemiplaneRiftOpenedComponent : Component
{
    /// <summary>
    /// The number of teleportations this teleporter can make before disappearing. Use the negative number to make infinite.
    /// </summary>
    [DataField]
    public int MaxUse = -1;

    [DataField]
    public float DoAfter = 4f;

    [DataField("arrivalSound")]
    public SoundSpecifier ArrivalSound = new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg");

    [DataField("departureSound")]
    public SoundSpecifier DepartureSound = new SoundPathSpecifier("/Audio/Effects/teleport_departure.ogg");
}
