namespace Content.Server._CP14.Expeditions.Components;

/// <summary>
/// Designates this entity as holding a expedition.
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class CP14ExpeditionComponent : Component
{
    [DataField]
    public EntityUid ExpeditionId;
}
