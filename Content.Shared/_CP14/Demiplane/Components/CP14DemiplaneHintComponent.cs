namespace Content.Shared._CP14.Demiplane.Components;

using Demiplane;

/// <summary>
/// A very small and silly component that simply turns the entity toward the nearest demiplane rift
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedDemiplaneSystem))]
public sealed partial class CP14DemiplaneHintComponent : Component
{
}
