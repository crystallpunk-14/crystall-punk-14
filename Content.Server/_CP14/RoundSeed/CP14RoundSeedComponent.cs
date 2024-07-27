namespace Content.Server._CP14.RoundSeed;

/// <summary>
/// This is used for round seed
/// </summary>
[RegisterComponent, Access(typeof(CP14RoundSeedSystem))]
public sealed partial class CP14RoundSeedComponent : Component
{
    public static int MaxValue = 10000;

    [DataField]
    public int Seed;
}
