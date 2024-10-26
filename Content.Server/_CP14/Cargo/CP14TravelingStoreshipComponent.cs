namespace Content.Server._CP14.Cargo;

[RegisterComponent, Access(typeof(CP14CargoSystem)), AutoGenerateComponentPause]
public sealed partial class CP14TravelingStoreShipComponent : Component
{
    [DataField]
    public EntityUid Station;
}
