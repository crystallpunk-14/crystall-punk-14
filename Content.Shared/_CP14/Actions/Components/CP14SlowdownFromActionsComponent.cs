using Robust.Shared.GameStates;

namespace Content.Shared._CP14.Actions.Components;

/// <summary>
/// apply slowdown effect from casting spells
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(CP14SharedActionSystem))]
public sealed partial class CP14SlowdownFromActionsComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<float> SpeedAffectors = new();
}
