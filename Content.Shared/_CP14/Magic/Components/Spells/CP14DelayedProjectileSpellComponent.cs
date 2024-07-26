using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Magic.Components.Spells;

/// <summary>
/// Stores a list of effects for delayed actions.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem))]
public sealed partial class CP14DelayedProjectileSpellComponent : Component
{
    /// <summary>
    /// What entity should be spawned.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Prototype;
}
