namespace Content.Shared._CP14.Actions.Components;

[RegisterComponent]
public sealed partial class CP14ActionEmotingComponent : Component
{
    [DataField]
    public string StartEmote = string.Empty; //Not LocId!

    [DataField]
    public string EndEmote = string.Empty; //Not LocId!
}
