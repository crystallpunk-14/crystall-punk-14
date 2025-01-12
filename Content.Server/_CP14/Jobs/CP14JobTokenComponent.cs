namespace Content.Server._CP14.Jobs;

/// <summary>
///
/// </summary>
[RegisterComponent]
public sealed partial class CP14JobTokenComponent : Component
{
    [DataField]
    public LocId? Description;
}
