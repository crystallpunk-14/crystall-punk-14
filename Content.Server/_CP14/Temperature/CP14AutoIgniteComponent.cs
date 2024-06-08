namespace Content.Server._CP14.Temperature;

[RegisterComponent, Access(typeof(CP14FireSpreadSystem))]
public sealed partial class CP14AutoIgniteComponent : Component
{
    [DataField]
    public float StartStack = 1f;
}
