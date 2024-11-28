using Robust.Shared.GameStates;

namespace Content.Shared._CP14.Damageable;

/// <summary>
/// Increases or decreases incoming damage, regardless of the damage type.
/// Unlike standard Damageable modifiers, this value can be changed during the game.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CP14DamageableModifierComponent : Component
{
    [DataField]
    public float Modifier = 1f;
}
