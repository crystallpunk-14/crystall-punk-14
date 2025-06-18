using Robust.Shared.GameStates;

namespace Content.Shared._CP14.Dash;

/// <summary>
/// This component marks entities that are currently in the dash
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(CP14DashSystem))]
public sealed partial class CP14DashComponent : Component
{
}
