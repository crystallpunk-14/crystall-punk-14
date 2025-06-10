using Content.Shared._CP14.Religion.Prototypes;
using Content.Shared._CP14.Religion.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Religion.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(CP14SharedReligionGodSystem))]
public sealed partial class CP14ReligionAltarComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<CP14ReligionPrototype>? Religion;

    [DataField, AutoNetworkedField]
    public bool CanBeConverted = true;
}
