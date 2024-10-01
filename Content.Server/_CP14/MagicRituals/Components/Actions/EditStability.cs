using Robust.Server.GameObjects;

namespace Content.Server._CP14.MagicRituals.Components.Actions;

public sealed partial class EditStability : CP14RitualAction
{
    [DataField(required: true)]
    public float Mod;

    public override void Effect(EntityManager entManager, TransformSystem _transform, Entity<CP14MagicRitualPhaseComponent> phase)
    {
        var _ritual = entManager.System<CP14RitualSystem>();

        if (phase.Comp.Ritual is not null)
            _ritual.ChangeRitualStability(phase.Comp.Ritual.Value, Mod);
    }
}
