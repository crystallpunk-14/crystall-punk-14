
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitual.Triggers;

public sealed partial class VoiceTrigger : CP14RitualTrigger
{
    [DataField]
    public string Message = string.Empty;

    [DataField]
    public int Speakers = 1;

    public override void Initialize(EntityManager entManager, Entity<CP14MagicRitualComponent> ritual)
    {

    }

    public override string? GetGuidebookTriggerDescription(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("cp14-ritual-trigger-voice", ("phrase", Message), ("count", Speakers));
    }
}
