using Content.Shared._CP14.MagicRitual;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitualTrigger.Triggers;

/// <summary>
/// Triggers when a creature passing the whitelist dies within range of the ritual.
/// </summary>
public sealed partial class CP14SacrificeWhitelistTrigger : CP14RitualTrigger
{
    [DataField]
    public float Range = 3f;

    [DataField(required: true)]
    public EntityWhitelist Whitelist = default!;

    [DataField(required: true)]
    public LocId WhitelistDesc = default!;

    public override void Initialize(EntityManager entManager, Entity<CP14MagicRitualPhaseComponent> ritual, RitualPhaseEdge edge)
    {
        entManager.EnsureComponent<CP14RitualSacrificeWhitelistTriggerComponent>(ritual, out var trigger);
        trigger.Triggers.Add(this);
        Edge = edge;
    }

    public override string? GetGuidebookTriggerDescription(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("cp14-ritual-trigger-sacrifice",
            ("name", Loc.GetString(WhitelistDesc)),
            ("range", Range));
    }
}
