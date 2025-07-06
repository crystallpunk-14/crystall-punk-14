namespace Content.Shared._CP14.MagicSpell.Components;

/// <summary>
/// If the user belongs to a religion, this spell can only be used within the area of influence of that religion
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem))]
public sealed partial class CP14MagicEffectReligionRestrictedComponent : Component
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
