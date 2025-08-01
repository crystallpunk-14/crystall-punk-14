namespace Content.Shared._CP14.Demiplane.Components;

/// <summary>
/// When closing the demiplane, all entities with this component will be thrown out instead of being deleted.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedDemiplaneSystem))]
public sealed partial class CP14DemiplaneForceExtractComponent : Component
{
    [DataField]
    public bool Enabled = true;
}
