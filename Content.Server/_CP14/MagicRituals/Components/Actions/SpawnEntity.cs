using System.Text;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.MagicRituals.Components.Actions;

/// <summary>
/// Creates an entity in the coordinates of the ritual.
/// </summary> TODO: EntityTable support?
public sealed partial class SpawnEntity : CP14RitualAction
{
    [DataField(required: true)]
    public Dictionary<EntProtoId, int> Spawns;

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

            sb.Append(Loc.GetString("cp14-ritual-effect-spawn-entity-item",
                ("name", Name is null ? indexed.Name : Loc.GetString(Name)),
                ("count", spawn.Value)) + "\n");
        }

        return sb.ToString();
    }

    public override void Effect(EntityManager entManager, TransformSystem _transform, Entity<CP14MagicRitualPhaseComponent> phase)
    {
        foreach (var spawn in Spawns)
        {
            for (var i = 0; i < spawn.Value; i++)
            {
                entManager.Spawn(spawn.Key, _transform.GetMapCoordinates(phase));
            }
        }
    }
}
