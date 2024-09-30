using Robust.Shared.Prototypes;

namespace Content.Server._CP14.MagicRituals.Components.Actions;

public sealed partial class SpawnEntity : CP14RitualAction
{
    [DataField(required: true)]
    public EntProtoId Proto = default!;

    public override void Effect(EntityManager entManager, EntityUid phaseEnt)
    {
        var _transform = entManager.System<SharedTransformSystem>();

        entManager.Spawn(Proto, _transform.GetMapCoordinates(phaseEnt));
    }
}
