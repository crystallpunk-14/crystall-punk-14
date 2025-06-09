using Content.Shared._CP14.Religion.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Religion.Components;

/// <summary>
/// Allows the god of a particular religion to see within a radius around the observer.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CP14ReligionObserverComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<CP14ReligionPrototype>, float> Observation = new(); //DAMNATION

    [DataField, AutoNetworkedField]
    public bool Active = true;
}
