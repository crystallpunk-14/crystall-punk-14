namespace Content.Shared._CP14.WorldEdge;

/// <summary>
/// when colliding with a player, starts a timer to remove him from the round.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedWorldEdgeSystem))]
public sealed partial class CP14WorldRemovePendingComponent : Component
{
    [DataField]
    public TimeSpan RemoveTime;

    [DataField]
    public Entity<CP14WorldBoundingComponent>? Bounding;
}
