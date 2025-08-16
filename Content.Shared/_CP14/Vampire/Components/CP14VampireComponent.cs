using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared.Body.Prototypes;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Vampire;

[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
[Access(typeof(CP14SharedVampireSystem))]
public sealed partial class CP14VampireComponent : Component
{
    [DataField]
    public ProtoId<ReagentPrototype> NewBloodReagent = "CP14BloodVampire";
    [DataField]
    public ProtoId<CP14SkillTreePrototype> SkillTreeProto = "NightChildren";

    [DataField]
    public ProtoId<MetabolizerTypePrototype> MetabolizerType = "CP14Vampire";

    [DataField]
    public ProtoId<CP14SkillPointPrototype> SkillPointProto = "Blood";

    [DataField, AutoNetworkedField]
    public ProtoId<FactionIconPrototype> FactionIcon = "CP14VampireNightChildrens";

    [DataField]
    public FixedPoint2 SkillPointCount = 3f;

    [DataField]
    public TimeSpan ToggleVisualsTime = TimeSpan.FromSeconds(2f);

    /// <summary>
    /// All this actions was granted to vampires on component added
    /// </summary>
    [DataField]
    public List<EntProtoId> ActionsProto = new() { "CP14ActionVampireToggleVisuals" };

    /// <summary>
    /// For tracking granted actions, and removing them when component is removed.
    /// </summary>
    [DataField]
    public List<EntityUid> Actions = new();

    [DataField]
    public float HeatUnderSunTemperature = 12000f;

    [DataField]
    public TimeSpan HeatFrequency = TimeSpan.FromSeconds(1);

    [DataField]
    public TimeSpan NextHeatTime = TimeSpan.Zero;

    [DataField]
    public float IgniteThreshold = 350f;

    public override bool SendOnlyToOwner => true;
}
