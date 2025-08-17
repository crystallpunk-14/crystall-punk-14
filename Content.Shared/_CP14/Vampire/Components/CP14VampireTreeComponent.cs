using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Vampire.Components;

[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
[Access(typeof(CP14SharedVampireSystem))]
public sealed partial class CP14VampireTreeComponent : Component
{
    [DataField, AutoNetworkedField]
    public FixedPoint2 CollectedEssence = 0f;

    [DataField]
    public ProtoId<CP14VampireFactionPrototype>? Faction;

    [DataField]
    public int? TreeLevel;

    [DataField]
    public FixedPoint2 EssenceToNextLevel = 0;

    [DataField]
    public EntProtoId? NextLevelProto;
}
