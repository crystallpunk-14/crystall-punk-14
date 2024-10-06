using Content.Shared._CP14.MagicRitual;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitualTrigger.Triggers;

/// <summary>
///
/// </summary>
public sealed partial class CP14SacrificeSpeciesTrigger : CP14RitualTrigger
{
    [DataField]
    public float Range = 3f;

    [DataField(required: true)]
    public ProtoId<SpeciesPrototype> Species = default!;

    public override void Initialize(EntityManager entManager, Entity<CP14MagicRitualPhaseComponent> ritual, RitualPhaseEdge edge)
    {
        entManager.EnsureComponent<CP14RitualSacrificeSpeciesTriggerComponent>(ritual, out var trigger);
        trigger.Triggers.Add(this);
        Edge = edge;
    }

    public override string? GetGuidebookTriggerDescription(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        if (!prototype.TryIndex(Species, out var indexedSpecies))
            return null;

        return Loc.GetString("cp14-ritual-trigger-sacrifice-species",
            ("species", Loc.GetString(indexedSpecies.Name)),
            ("range", Range));
    }
}
