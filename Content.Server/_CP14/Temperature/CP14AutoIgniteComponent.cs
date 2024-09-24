namespace Content.Server._CP14.Temperature;

[RegisterComponent, AutoGenerateComponentPause, Access(typeof(CP14FireSpreadSystem))]
public sealed partial class CP14AutoIgniteComponent : Component
{
    [DataField]
    public float StartStack = 1f;

    [DataField]
    public TimeSpan IgniteDelay = TimeSpan.FromSeconds(1f);

    [DataField]
    public TimeSpan IgniteTime = TimeSpan.Zero;
}
