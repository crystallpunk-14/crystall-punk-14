using System.Text;
using Content.Shared._CP14.MagicRitual.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitual.Actions;

/// <summary>
/// Removes the orb key from the ritual.
/// </summary>
public sealed partial class ConsumeOrb : CP14RitualAction
{
    [DataField(required: true)]
    public ProtoId<CP14MagicTypePrototype> MagicType = new();

    [DataField(required: true)]
    public int Count = 0;

    public override string? GetGuidebookEffectDescription(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        if (!prototype.TryIndex(MagicType, out var indexedType))
            return null;

        var sb = new StringBuilder();
        sb.Append(Loc.GetString("cp14-ritual-effect-consume-orb", ("name", Loc.GetString(indexedType.Name)), ("count", Count))+ "\n");

        return sb.ToString();
    }

    public override void Effect(EntityManager entManager, SharedTransformSystem transform, Entity<CP14MagicRitualPhaseComponent> phase)
    {
        if (phase.Comp.Ritual is null)
            return;

        var ritual = entManager.System<CP14SharedRitualSystem>();

        for (var i = 0; i < Count; i++)
        {
            ritual.ConsumeOrbType(phase.Comp.Ritual.Value, MagicType);
        }
    }
}
