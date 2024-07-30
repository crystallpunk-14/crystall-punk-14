using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Magic.Components.Spells;

/// <summary>
/// Stores a list of effects for delayed actions.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem))]
public sealed partial class CP14DelayedSpawnEntitiesSpellComponent : Component
{
    /// <summary>
    /// What entities should be spawned.
    /// </summary>
    [DataField(required: true)]
    public HashSet<EntProtoId> Spawns = new();
}
