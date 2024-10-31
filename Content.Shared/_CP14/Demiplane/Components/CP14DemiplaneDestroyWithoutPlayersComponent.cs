namespace Content.Shared._CP14.Demiplane.Components;

/// <summary>
/// Automatically deletes the demiplane if everyone who entered it, exited back out, or died.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedDemiplaneSystem))]
public sealed partial class CP14DemiplaneDestroyWithoutPlayersComponent : Component
{
    [DataField]
    public HashSet<EntityUid> Players = new();
}
