namespace Content.Shared._CP14.Actions.Components;

/// <summary>
/// Requires the user to have at least one free hand to use this spell
/// </summary>
[RegisterComponent]
public sealed partial class CP14ActionFreeHandsRequiredComponent : Component
{
    [DataField]
    public int FreeHandRequired = 1;
}
