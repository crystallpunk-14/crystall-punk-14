using Content.Shared._CP14.Religion.Prototypes;
using Content.Shared._CP14.Religion.Systems;
using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Religion.Components;

/// <summary>
/// This entity has not yet become a follower of God, but wants to become one. Confirmation from god is expected
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(CP14SharedReligionGodSystem))]
public sealed partial class CP14ReligionPendingFollowerComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<CP14ReligionPrototype>? Religion;
}
