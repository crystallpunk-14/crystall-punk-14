namespace Content.Shared._CP14.DayCycle;


[RegisterComponent]
public sealed partial class CP14DayCycleComponent : Component
{
    public double LastLightLevel = 0f;

    [DataField]
    public double Threshold = 0.5f;
}
