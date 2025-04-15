namespace Content.Shared._CP14.MagicSpell.Components;

/// <summary>
/// Requires the user to be able to speak in order to use this spell. Also forces the user to use certain phrases at the beginning and end of a spell cast
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem))]
public sealed partial class CP14MagicEffectEmotingComponent : Component
{
    [DataField]
    public string StartEmote = string.Empty; //Not LocId!

    [DataField]
    public string EndEmote = string.Empty; //Not LocId!
}
