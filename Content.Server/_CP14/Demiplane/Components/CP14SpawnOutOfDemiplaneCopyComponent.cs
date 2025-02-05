using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Demiplane.Components;

/// <summary>
/// Create copy of this entity on initialization, if entity created in demiplane map
/// </summary>
[RegisterComponent]
public sealed partial class CP14SpawnOutOfDemiplaneComponent : Component
{
    [DataField(required: true)]
    public EntProtoId Proto;
}
