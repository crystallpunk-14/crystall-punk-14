using Content.Shared._CP14.Religion.Prototypes;
using Content.Shared._CP14.Religion.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Religion.Components;

/// <summary>
/// Allows the god of a particular religion to see within a radius around the observer.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), Access(typeof(CP14SharedReligionGodSystem))]
public sealed partial class CP14ReligionObserverComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<CP14ReligionPrototype>? Religion;

    [DataField, AutoNetworkedField]
    public float Radius = 5f;

    [DataField, AutoNetworkedField]
    public bool Active = true;
}
