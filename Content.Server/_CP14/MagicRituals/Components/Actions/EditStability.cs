using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

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

    public override string? GetGuidebookEffectDescription(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Mod switch
        {
            > 0 => Loc.GetString("cp14-ritual-effect-stability-add", ("count", Mod * 100)) + "\n",
            < 0 => Loc.GetString("cp14-ritual-effect-stability-minus", ("count", -Mod * 100)) + "\n",
            _ => null,
        };
    }
}
