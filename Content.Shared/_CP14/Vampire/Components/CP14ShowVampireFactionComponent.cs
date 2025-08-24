using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Vampire.Components;

[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class CP14ShowVampireFactionComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<CP14VampireFactionPrototype>? Faction;
}
