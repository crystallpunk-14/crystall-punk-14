using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitual;

public sealed class CP14RitualTriggerEvent : EntityEventArgs
{
    public EntProtoId? NextPhase;

    public CP14RitualTriggerEvent(EntProtoId phase)
    {
        NextPhase = phase;
    }
}
