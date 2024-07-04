namespace Content.Shared._CP14.WorldEdge;

/// <summary>
/// when colliding with a player, starts a timer to remove him from the round.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedWorldEdgeSystem))]
public sealed partial class CP14WorldBoundingComponent : Component
{
    [DataField]
    public TimeSpan ReturnTime = TimeSpan.FromSeconds(15f);

    [DataField]
    public float Range = 0f;
}
