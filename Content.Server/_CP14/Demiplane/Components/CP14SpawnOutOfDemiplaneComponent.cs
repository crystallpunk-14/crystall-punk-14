using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Demiplane.Components;

/// <summary>
/// Creates an entity on demiplane exit points when that entity appears.
/// </summary>
[RegisterComponent]
public sealed partial class CP14SpawnOutOfDemiplaneComponent : Component
{
    /// <summary>
    /// If null, the ProtoId of this entity is taken from the entity itself.
    /// </summary>
    [DataField]
    public EntProtoId? Proto;
}
