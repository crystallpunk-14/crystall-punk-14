using System.Text;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitual.Actions;

/// <summary>
///
/// </summary>
public sealed partial class AddOrb : CP14RitualAction
{
    [DataField(required: true)]
    public Dictionary<EntProtoId, int> Orbs = new();

    public override string? GetGuidebookEffectDescription(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        var sb = new StringBuilder();
        sb.Append(Loc.GetString("cp14-ritual-effect-add-orb")+ "\n");
        foreach (var orb in Orbs)
        {
            if (!prototype.TryIndex(orb.Key, out var indexedOrb))
                continue;

            sb.Append(Loc.GetString("cp14-ritual-entry-item", ("name", indexedOrb.Name), ("count", orb.Value)) + "\n");
        }

        return sb.ToString();
    }

    public override void Effect(EntityManager entManager, SharedTransformSystem _transform, Entity<CP14MagicRitualPhaseComponent> phase)
    {
        if (phase.Comp.Ritual is null)
            return;

        var _ritual = entManager.System<CP14SharedRitualSystem>();

        foreach (var orb in Orbs)
        {
            for (var i = 0; i < orb.Value; i++)
            {
                _ritual.AddOrbToRitual(phase.Comp.Ritual.Value, orb.Key);
            }
        }
    }
}
