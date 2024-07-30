/*
 * All right reserved to CrystallPunk.
 *
 * BUT this file is sublicensed under MIT License
 *
 */

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
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

    [PublicAPI]
    public bool TryGetSeed([NotNullWhen(true)] out int? seed)
    {
        seed = null;
        var query = EntityQuery<CP14RoundSeedComponent>();
        foreach (var comp in query)
        {
            seed = comp.Seed;
            return true;
        }

        return false;
    }
}
