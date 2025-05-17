using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.BloodMoon;


[RegisterComponent, NetworkedComponent]
public sealed partial class CP14BloodMoonCurseComponent : Component
{
    [DataField]
    public EntityUid? CurseRule;

    [DataField]
    public EntProtoId CurseEffect = "CP14BloodMoonCurseEffect";

    [DataField]
    public EntityUid? SpawnedEffect;

    [DataField]
    public TimeSpan EndStunDuration = TimeSpan.FromSeconds(60f);
}
