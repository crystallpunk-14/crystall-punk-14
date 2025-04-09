namespace Content.Shared._CP14.MagicSpell.Components;

/// <summary>
/// Requires the user to be able to speak in order to use this spell. Also forces the user to use certain phrases at the beginning and end of a spell cast
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem))]
public sealed partial class CP14MagicEffectVerbalAspectComponent : Component
{
    [DataField]
    public string StartSpeech = string.Empty; //Not LocId!

    [DataField]
    public string EndSpeech = string.Empty; //Not LocId!

    [DataField]
    public bool Emote = false;
}

/// <summary>
/// patch to send an event to the server for saying a phrase out loud
/// </summary>
[ByRefEvent]
public sealed class CP14VerbalAspectSpeechEvent : EntityEventArgs
{
    public EntityUid? Performer { get; init; }

    public string? Speech { get; init; }

    public bool Emote { get; init; }
}
