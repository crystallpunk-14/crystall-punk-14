namespace Content.Shared._CP14.DayCycle;


[RegisterComponent]
public sealed partial class CP14DayCycleComponent : Component
{
    public float LastLightLevel = 0f;

    [DataField]
    public float Threshold = 0.5f;
}
