namespace Content.Server._CP14.Farming.Components;

[RegisterComponent, Access(typeof(CP14FarmingSystem))]
public sealed partial class CP14SoilComponent : Component
{
    [DataField(required: true)]
    public string Solution = string.Empty;

}
