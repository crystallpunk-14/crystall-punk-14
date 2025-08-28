using Robust.Shared.GameStates;

namespace Content.Shared._CP14.IdentityRecognition;

/// <summary>
/// Stores all the names of other characters that the player has memorized.
/// These players will be visible to the player under that name, rather than as nameless characters.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CP14RememberedNamesComponent : Component
{
    //Pair of NetEntity Id and names
    [DataField, AutoNetworkedField]
    public Dictionary<int, string> Names = [];
}
