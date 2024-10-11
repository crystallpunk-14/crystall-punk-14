namespace Content.Shared._CP14.TravelingStoreShip;

/// <summary>
/// Allows users to view information on city trading opportunities
/// </summary>
[RegisterComponent]
public sealed partial class CP14CargoStoreComponent : Component
{
    [DataField]
    public Entity<CP14StationTravelingStoreshipTargetComponent>? Station = null;
}
