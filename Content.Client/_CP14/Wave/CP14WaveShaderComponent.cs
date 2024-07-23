namespace Content.Client._CP14.Wave;

[RegisterComponent]
[Access(typeof(CP14WaveShaderSystem))]
public sealed partial class CP14WaveShaderComponent : Component
{
    [DataField]
    public float Speed = 10f;

    [DataField]
    public float Dis = 10f;

    [DataField]
    public float Offset = 0f;
}
