using Content.Shared.FixedPoint;

namespace Content.Shared._CP14.Action.Components;

/// <summary>
/// Restricts the use of this action, by spending mana or user requirements.
/// </summary>
[RegisterComponent]
public sealed partial class CP14ActionManaCostComponent : Component
{
    [DataField]
    public FixedPoint2 ManaCost = 0f;

    /// <summary>
    /// Can the cost of casting this magic effect be changed from clothing or other sources?
    /// </summary>
    [DataField]
    public bool CanModifyManacost = true;
}
