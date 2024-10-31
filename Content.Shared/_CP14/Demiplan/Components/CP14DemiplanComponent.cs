using Robust.Shared.Audio;

namespace Content.Shared._CP14.Demiplan.Components;

/// <summary>
/// Designates this entity as holding a demiplan.
/// </summary>
[RegisterComponent]
public sealed partial class CP14DemiplanComponent : Component
{
    /// <summary>
    /// All entities in the real world that are connected to this demiplane
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField]
    public HashSet<Entity<CP14DemiplanRiftComponent>> ExitPoints = new();

    /// <summary>
    /// All entities in the demiplane in which the objects entered in the demiplane appear
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField]
    public HashSet<Entity<CP14DemiplanRiftComponent>> EntryPoints = new();

    /// <summary>
    ///  The sound of entering a demiplane, played locally to the player who entered it.
    ///  Consider more as an intro sound “You have entered the demiplane. Good luck.”
    /// </summary>
    [DataField("arrivalSound")]
    public SoundSpecifier ArrivalSound = new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg");

    /// <summary>
    ///  The sound of exiting the demiplane, played locally to the player who exited the demiplane.
    /// Consider it more as an ending sound
    /// </summary>
    [DataField("departureSound")]
    public SoundSpecifier DepartureSound = new SoundPathSpecifier("/Audio/Effects/teleport_departure.ogg");
}
