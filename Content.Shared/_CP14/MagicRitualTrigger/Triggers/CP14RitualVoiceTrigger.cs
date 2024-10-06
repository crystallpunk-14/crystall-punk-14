using Content.Shared._CP14.MagicRitual;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitualTrigger.Triggers;

/// <summary>
/// Triggers a phase transition when the Ritual hears a certain message
/// </summary>
public sealed partial class CP14VoiceTrigger : CP14RitualTrigger
{
    [DataField]
    public string Message = string.Empty;

    [DataField]
    public int Speakers = 1;

    public override void Initialize(EntityManager entManager, Entity<CP14MagicRitualPhaseComponent> ritual, RitualPhaseEdge edge)
    {
        entManager.EnsureComponent<CP14RitualVoiceTriggerComponent>(ritual, out var trigger);
        trigger.Triggers.Add(this);
        Edge = edge;
    }

    public override string? GetGuidebookTriggerDescription(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("cp14-ritual-trigger-voice", ("phrase", Message), ("count", Speakers));
    }
}
