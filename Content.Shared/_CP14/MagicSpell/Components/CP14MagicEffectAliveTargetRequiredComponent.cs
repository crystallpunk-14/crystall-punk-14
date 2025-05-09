using Robust.Shared.GameStates;

namespace Content.Shared._CP14.MagicSpell.Components;

/// <summary>
/// Allows you to limit the use of a spell based on the target's alive/dead status
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CP14MagicEffectAliveTargetRequiredComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Inverted = false;
}
