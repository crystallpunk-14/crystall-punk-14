using Content.Shared._CP14.MagicRitual;
using Content.Shared._CP14.MagicRitualTrigger;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.MagicRitualTrigger;

public partial class CP14RitualTriggerSystem : CP14SharedRitualTriggerSystem
{
    public override void Initialize()
    {
        InitializeTimer();
        InitializeVoice();
        InitializeSacrifice();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateTimer(frameTime);
    }

    private void TriggerRitualPhase(Entity<CP14MagicRitualPhaseComponent> ent, EntProtoId nextPhase)
    {
        var evConfirmed = new CP14RitualTriggerEvent(nextPhase);
        RaiseLocalEvent(ent, evConfirmed);
    }
}
