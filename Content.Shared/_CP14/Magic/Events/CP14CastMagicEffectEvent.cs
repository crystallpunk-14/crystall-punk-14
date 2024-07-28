namespace Content.Shared._CP14.Magic.Events;

[ByRefEvent]
public sealed class CP14BeforeCastMagicEffectEvent : CancellableEntityEventArgs
{
    /// <summary>
    /// The Performer of the event, to check if they meet the requirements.
    /// </summary>
    public EntityUid Performer { get; init; }

    public string Reason = string.Empty;

    public void PushReason(string reason)
    {
        Reason += $"{reason}\n";
    }
}

[ByRefEvent]
public sealed class CP14AfterCastMagicEffectEvent : EntityEventArgs
{
    public EntityUid? Performer { get; init; }
}
/// <summary>
/// is invoked if all conditions are met and the spell has begun to be cast
/// </summary>
[ByRefEvent]
public sealed class CP14StartCastMagicEffectEvent : EntityEventArgs
{
    public EntityUid Performer { get; init; }
}

/// <summary>
/// is invoked on the spell itself when the spell process has been completed or interrupted
/// </summary>
[ByRefEvent]
public sealed class CP14StopCastMagicEffectEvent : EntityEventArgs
{
}


