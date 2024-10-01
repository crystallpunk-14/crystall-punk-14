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

    public override void Effect(EntityManager entManager, TransformSystem _transform, Entity<CP14MagicRitualPhaseComponent> phase)
    {
        entManager.Spawn(Proto, _transform.GetMapCoordinates(phase));
        entManager.Spawn(VisualEffect, _transform.GetMapCoordinates(phase));
    }
}
