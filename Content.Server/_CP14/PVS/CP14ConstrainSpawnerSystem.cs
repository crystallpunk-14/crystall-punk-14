using System.Numerics;
using Content.Server.Explosion.EntitySystems;
using Content.Server.GameTicking;
using Content.Shared.Ghost;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using MyNamespace;
using Robust.Shared.Random;

namespace Content.Server._CP14.PVS;

public sealed class CP14ConstrainSpawnerSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly GameTicker _ticker = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14ConstrainedSpawnerOnTriggerComponent, TriggerEvent>(OnSpawnTrigger);
    }

    private void OnSpawnTrigger(Entity<CP14ConstrainedSpawnerOnTriggerComponent> spawner, ref TriggerEvent args)
    {
        if (HasComp<GhostComponent>(args.User))//TODO replace with Whitelist system?
            return;

        if (!TryComp<MindContainerComponent>(args.User, out var mind))
            return;

        if (!mind.HasMind)
            return;

        TrySpawn(spawner);
    }

    private void UpdateSpawned(Entity<CP14ConstrainedSpawnerOnTriggerComponent> spawner)
    {
        foreach (var spawned in spawner.Comp.Spawned)
        {
            if (!EntityManager.EntityExists(spawned))
                spawner.Comp.Spawned.Remove(spawned);
        }
    }

    private bool CanSpawn(Entity<CP14ConstrainedSpawnerOnTriggerComponent> spawner)
    {
        UpdateSpawned(spawner);
        return spawner.Comp.Spawned.Count < spawner.Comp.MaxCount;
    }

    private void TrySpawn(Entity<CP14ConstrainedSpawnerOnTriggerComponent> spawner)
    {
        if (CanSpawn(spawner))
            Spawn(spawner);
    }

    private void Spawn(Entity<CP14ConstrainedSpawnerOnTriggerComponent> spawner)
    {
        if (!_random.Prob(spawner.Comp.Chance))
            return;

        if (spawner.Comp.Prototypes.Count == 0)
        {
            Log.Warning($"Prototype list in ConditionalSpawnComponent is empty! Entity: {ToPrettyString(spawner)}");
            return;
        }

        if (Deleted(spawner))
            return;

        var offset = spawner.Comp.Offset;
        var xOffset = _random.NextFloat(-offset, offset);
        var yOffset = _random.NextFloat(-offset, offset);

        var coordinates = Transform(spawner).Coordinates.Offset(new Vector2(xOffset, yOffset));

        spawner.Comp.Spawned.Add(EntityManager.SpawnEntity(_random.Pick(spawner.Comp.Prototypes), coordinates));
    }
}
