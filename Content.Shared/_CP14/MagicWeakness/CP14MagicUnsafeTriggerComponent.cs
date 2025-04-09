using Robust.Shared.GameStates;

namespace Content.Shared._CP14.MagicWeakness;

/// <summary>
/// trigger entity on unsafe magic energy damage
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(CP14SharedMagicWeaknessSystem))]
public sealed partial class CP14MagicUnsafeTriggerComponent : Component
{
}
