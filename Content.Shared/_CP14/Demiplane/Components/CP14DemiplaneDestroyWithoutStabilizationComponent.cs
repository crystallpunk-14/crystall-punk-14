namespace Content.Shared._CP14.Demiplane.Components;

/// <summary>
/// is automatically delete over time if there are no active stabilizers inside this demiplane.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedDemiplaneSystem)), AutoGenerateComponentPause]
public sealed partial class CP14DemiplaneDestroyWithoutStabilizationComponent : Component
{
}
