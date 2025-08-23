using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Vampire;

[RegisterComponent, NetworkedComponent]
public sealed partial class CP14VampireVisualsComponent : Component
{
    [DataField]
    public Color EyesColor = Color.Red;

    [DataField]
    public Color OriginalEyesColor = Color.White;

    [DataField]
    public string FangsMap = "vampire_fangs";

    [DataField]
    public EntProtoId EnableVFX = "CP14ImpactEffectBloodEssence2";

    [DataField]
    public EntProtoId DisableVFX = "CP14ImpactEffectBloodEssenceInverse";
}
