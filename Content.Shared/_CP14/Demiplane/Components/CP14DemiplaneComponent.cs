using Robust.Shared.Audio;

namespace Content.Shared._CP14.Demiplane.Components;

/// <summary>
/// Designates this entity as holding a demiplane.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedDemiplaneSystem))]
public sealed partial class CP14DemiplaneComponent : Component
{
    /// <summary>
    /// All entities in the real world that are connected to this demiplane
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField]
    public HashSet<EntityUid> ExitPoints = new();

    /// <summary>
    /// All entities in the demiplane in which the objects entered in the demiplane appear
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField]
    public HashSet<EntityUid> EntryPoints = new();

    /// <summary>
    ///  The sound of entering a demiplane, played locally to the player who entered it.
    ///  Consider more as an intro sound “You have entered the demiplane. Good luck.”
    /// </summary>
    [DataField("arrivalSound")]
    public SoundSpecifier ArrivalSound = new SoundCollectionSpecifier("CP14DemiplaneIntro");

    /// <summary>
    ///  The sound of exiting the demiplane, played locally to the player who exited the demiplane.
    /// Consider it more as an ending sound
    /// </summary>
    [DataField("departureSound")]
    public SoundSpecifier DepartureSound = new SoundCollectionSpecifier("CP14DemiplaneIntro");
}
