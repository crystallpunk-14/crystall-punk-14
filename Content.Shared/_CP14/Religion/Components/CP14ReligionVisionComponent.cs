using Content.Shared._CP14.Religion.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Religion.Components;

/// <summary>
/// Limits the vision of entities, allowing them to see only areas within a radius around observers of their religion.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CP14ReligionVisionComponent : Component
{
    [DataField(required: true)]
    public ProtoId<CP14ReligionPrototype>? Religion;
}
