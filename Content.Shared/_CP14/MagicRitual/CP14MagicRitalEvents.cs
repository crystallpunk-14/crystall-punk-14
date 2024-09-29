using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitual;

public sealed class CP14RitualTriggerAttempt : CancellableEntityEventArgs
{
    public EntProtoId? NextPhase;

    public CP14RitualTriggerAttempt(EntProtoId phase)
    {
        NextPhase = phase;
    }
}
