namespace Content.Shared._CP14.Actions.Components;

/// <summary>
/// Requires the user to be able to speak in order to use this action. Also forces the user to use certain phrases at the beginning and end of a action use
/// </summary>
[RegisterComponent]
public sealed partial class CP14ActionSpeakingComponent : Component
{
    [DataField]
    public string StartSpeech = string.Empty; //Not LocId!

    [DataField]
    public string EndSpeech = string.Empty; //Not LocId!

    [DataField]
    public bool Whisper = false;
}
