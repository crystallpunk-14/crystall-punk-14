using Robust.Shared.GameStates;

namespace Content.Shared._CP14.IdentityRecognition;

/// <summary>
/// defines this character's name as unknown.
/// The name can be memorized via KnownNamesComponent,
/// and is hidden when IdentityBlocker is enabled.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CP14UnknownIdentityComponent : Component
{
}
