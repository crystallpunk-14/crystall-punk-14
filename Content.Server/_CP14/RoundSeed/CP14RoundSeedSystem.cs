/*
 * All right reserved to CrystallPunk.
 *
 * BUT this file is sublicensed under MIT License
 *
 */

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server._CP14.RoundSeed;

/// <summary>
/// Provides a round seed for another systems
/// </summary>
public sealed class CP14RoundSeedSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CP14RoundSeedComponent, ComponentStartup>(OnComponentStartup);
    }

    private void OnComponentStartup(Entity<CP14RoundSeedComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.Seed = _random.Next(CP14RoundSeedComponent.MaxValue);
    }

    private int SetupSeed()
    {
        return AddComp<CP14RoundSeedComponent>(Spawn(null, MapCoordinates.Nullspace)).Seed;
    }

    /// <summary>
    /// Returns the round seed if assigned, otherwise assigns the round seed itself.
    /// </summary>
    /// <returns>seed of the round</returns>
    public int GetSeed()
    {
        var query = EntityQuery<CP14RoundSeedComponent>();
        foreach (var comp in query)
        {
            return comp.Seed;
        }

        var seed = SetupSeed();
        Log.Warning($"Missing RoundSeed. Seed set to {seed}");
        return seed;
    }
}
