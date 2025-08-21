using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Vampire.Components;

[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
[Access(typeof(CP14SharedVampireSystem))]
public sealed partial class CP14VampireClanHeartComponent : Component
{
    [DataField, AutoNetworkedField]
    public FixedPoint2 CollectedEssence = 0f;

    [DataField]
    public string LevelPrefix = "orb";

    [DataField]
    public ProtoId<CP14VampireFactionPrototype>? Faction;

    [DataField]
    public FixedPoint2 Level2 = 5f;

    [DataField]
    public FixedPoint2 Level3 = 10f;

    [DataField]
    public FixedPoint2 Level4 = 20f;

    public int Level
    {
        get
        {
            if (CollectedEssence >= Level4)
                return 4;
            if (CollectedEssence >= Level3)
                return 3;
            if (CollectedEssence >= Level2)
                return 2;
            return 1;
        }
    }

    public FixedPoint2 EssenceRequiredForLevel => Level switch
    {
        1 => Level2,
        2 => Level3,
        3 => Level4,
        _ => FixedPoint2.Zero
    };

    public FixedPoint2 EssenceFromLevelStart => Level switch
    {
        1 => CollectedEssence,
        2 => CollectedEssence - Level2,
        3 => CollectedEssence - Level3,
        4 => CollectedEssence - Level4,
        _ => FixedPoint2.Zero
    };

    public FixedPoint2? EssenceToNextLevel => Level switch
    {
        1 => Level2,
        2 => Level3 - Level2,
        3 => Level4 - Level3,
        _ => null
    };
}
