namespace Content.Server._CP14.Farming;

[RegisterComponent, Access(typeof(CP14FarmingSystem))]
public sealed partial class CP14SeedbedComponent : Component
{
    [DataField(required: true)]
    public string Solution = string.Empty;

}
