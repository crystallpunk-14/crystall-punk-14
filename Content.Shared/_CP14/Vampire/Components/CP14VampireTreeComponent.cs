using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Vampire.Components;

[RegisterComponent]
[NetworkedComponent]
[Access(typeof(CP14SharedVampireSystem))]
public sealed partial class CP14VampireTreeComponent : Component
{
    [DataField]
    public FixedPoint2 CollectedEssence = 0f;

    [DataField(required: true)]
    public int TreeLevel = 0;

    [DataField(required: true)]
    public FixedPoint2 EssenceToNextLevel = 0;

    [DataField(required: true)]
    public EntProtoId NextLevelProto = string.Empty;
}
