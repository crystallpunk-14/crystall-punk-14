using System.Text;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitual.Actions;

/// <summary>
/// Creates an entity in the coordinates of the ritual.
/// </summary> TODO: EntityTable support?
public sealed partial class SpawnEntity : CP14RitualAction
{
    [DataField(required: true)]
    public Dictionary<EntProtoId, int> Spawns = new();

    [DataField]
    public LocId? Name;

    public override string? GetGuidebookEffectDescription(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        var sb = new StringBuilder();
        sb.Append(Loc.GetString("cp14-ritual-effect-spawn-entity")+ "\n");
        foreach (var spawn in Spawns)
        {
            if (!prototype.TryIndex(spawn.Key, out var indexed))
                return null;

            sb.Append(Loc.GetString("cp14-ritual-entry-item",
                ("name", Name is null ? indexed.Name : Loc.GetString(Name)),
                ("count", spawn.Value)) + "\n");
        }

        return sb.ToString();
    }

    public override void Effect(EntityManager entManager, SharedTransformSystem transform, Entity<CP14MagicRitualPhaseComponent> phase)
    {
        foreach (var spawn in Spawns)
        {
            for (var i = 0; i < spawn.Value; i++)
            {
                entManager.Spawn(spawn.Key, transform.GetMapCoordinates(phase));
            }
        }
    }
}
