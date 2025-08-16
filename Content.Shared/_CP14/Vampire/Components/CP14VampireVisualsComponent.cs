using Robust.Shared.GameStates;

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
}
