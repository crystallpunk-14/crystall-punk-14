/*
 * All right reserved to CrystallPunk.
 *
 * BUT this file is sublicensed under MIT License
 *
 */

namespace Content.Server._CP14.RoundSeed;

/// <summary>
/// This is used for round seed
/// </summary>
[RegisterComponent, Access(typeof(CP14RoundSeedSystem))]
public sealed partial class CP14RoundSeedComponent : Component
{
    [ViewVariables]
    public static int MaxValue = 10000;

    [ViewVariables]
    public int Seed;
}
