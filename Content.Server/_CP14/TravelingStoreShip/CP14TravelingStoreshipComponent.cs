namespace Content.Server._CP14.TravelingStoreShip;

[RegisterComponent, Access(typeof(CP14TravelingStoreShipSystem)), AutoGenerateComponentPause]
public sealed partial class CP14TravelingStoreShipComponent : Component
{
    [DataField]
    public EntityUid Station;
}
