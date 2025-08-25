
namespace Content.Shared._CP14.Action.Components;

/// <summary>
/// If the user belongs to a religion, this spell can only be used within the area of influence of that religion
/// </summary>
[RegisterComponent]
public sealed partial class CP14ActionReligionRestrictedComponent : Component
{
    /// <summary>
    /// does not allow the spell to be used outside the god's area of influence
    /// </summary>
    [DataField]
    public bool OnlyInReligionZone = true;

    /// <summary>
    /// allows the spell to be used only on followers
    /// </summary>
    [DataField]
    public bool OnlyOnFollowers = false;
}
