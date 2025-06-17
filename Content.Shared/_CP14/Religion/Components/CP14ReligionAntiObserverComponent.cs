using Content.Shared._CP14.Religion.Prototypes;
using Content.Shared._CP14.Religion.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Religion.Components;

/// <summary>
/// Blocks divine sight in a radius around this entity
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), Access(typeof(CP14SharedReligionGodSystem))]
public sealed partial class CP14ReligionAntiObserverComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<CP14ReligionPrototype>, float> Observation = new(); //DAMNATION

    [DataField, AutoNetworkedField]
    public bool Active = true;

    [DataField]
    public float GlobalObservation = 0f;
}
