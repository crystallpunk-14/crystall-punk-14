using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.MagicRituals.Components.Actions;

/// <summary>
/// Creates an entity in the coordinates of the ritual.
/// </summary> TODO: EntityTable support?
public sealed partial class SpawnEntity : CP14RitualAction
{
    [DataField(required: true)]
    public EntProtoId Proto;

    [DataField]
    public int Count = 1;

    [DataField]
    public LocId? Name;

    public override string? GetGuidebookEffectDescription(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        if (!prototype.TryIndex(Proto, out var indexed))
            return null;

        return Loc.GetString("cp14-ritual-effect-spawn-entity",
            ("name", Name is null ? indexed.Name : Loc.GetString(Name)),
            ("count", Count)) + "\n";
    }

    public override void Effect(EntityManager entManager, TransformSystem _transform, Entity<CP14MagicRitualPhaseComponent> phase)
    {
        for (var i = 0; i < Count; i++)
        {
            entManager.Spawn(Proto, _transform.GetMapCoordinates(phase));
        }

        entManager.Spawn(VisualEffect, _transform.GetMapCoordinates(phase));
    }
}
