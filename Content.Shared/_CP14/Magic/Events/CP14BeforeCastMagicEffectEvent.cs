namespace Content.Shared._CP14.Magic.Events;

[ByRefEvent]
public sealed class CP14BeforeCastMagicEffectEvent : CancellableEntityEventArgs
{
    /// <summary>
    /// The Performer of the event, to check if they meet the requirements.
    /// </summary>
    public EntityUid? Caster { get; init; }
}
