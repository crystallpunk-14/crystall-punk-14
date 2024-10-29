using Content.Shared._CP14.Expeditions;

namespace Content.Server._CP14.Expeditions.Components;

/// <summary>
/// Designates this entity as holding a expedition.
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class CP14ExpeditionComponent : Component
{
    public CP14ExpeditionMissionParams MissionParams = default!;
}
