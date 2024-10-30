using Robust.Shared.Audio;

namespace Content.Shared._CP14.Demiplan.Components;

/// <summary>
/// allows you to travel between demiplanes via doAfter
/// </summary>
[RegisterComponent]
public sealed partial class CP14DemiplanPasswayComponent : Component
{
    /// <summary>
    /// The number of teleportations this teleporter can make before disappearing. Use the negative number to make infinite.
    /// </summary>
    [DataField]
    public int MaxUse = -1;

    [DataField]
    public float DoAfter = 4f;

    /// <summary>
    /// Completely undresses and removes all items from the character before teleportation
    /// </summary>
    [DataField]
    public bool DidItNude = false;

    /// <summary>
    ///     Sound played on arriving to this portal, centered on the destination.
    ///     The arrival sound of the entered portal will play if the destination is not a portal.
    /// </summary>
    [DataField("arrivalSound")]
    public SoundSpecifier ArrivalSound = new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg");

    /// <summary>
    ///     Sound played on departing from this portal, centered on the original portal.
    /// </summary>
    [DataField("departureSound")]
    public SoundSpecifier DepartureSound = new SoundPathSpecifier("/Audio/Effects/teleport_departure.ogg");
}
