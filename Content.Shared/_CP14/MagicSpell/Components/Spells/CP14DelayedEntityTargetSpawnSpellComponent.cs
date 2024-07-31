using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Components.Spells;

[RegisterComponent, Access(typeof(CP14SharedMagicSystem))]
public sealed partial class CP14DelayedEntityTargetSpawnSpellComponent : Component
{
    /// <summary>
    /// What entities should be spawned.
    /// </summary>
    [DataField(required: true)]
    public HashSet<EntProtoId> Spawns = new();
}
