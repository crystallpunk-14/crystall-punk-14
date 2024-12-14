using Content.Shared._CP14.Fishing.Components;
using Content.Shared._CP14.Fishing.Core;
using Content.Shared._CP14.Fishing.Core.Behaviors;
using Content.Shared._CP14.Fishing.Events;
using Content.Shared._CP14.Fishing.Systems;
using Content.Shared.Throwing;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Fishing;

public sealed class CP14FishingProcessSystem : CP14SharedFishingProcessSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly CP14FishingPoolSystem _pool = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14FishingProcessComponent>();
        while (query.MoveNext(out var entityUid, out var processComponent))
        {
            Update((entityUid, processComponent), frameTime);
        }
    }

    public Entity<CP14FishingProcessComponent> Start(
        Entity<CP14FishingRodComponent> fishingRod,
        Entity<CP14FishingPoolComponent> fishingPool,
        EntityUid user)
    {
        var process = CreateProcess(fishingRod);
        var loot = _pool.GetLootPrototypeId(fishingPool);
        var style = GetStyle(fishingRod);

        // Save entities
        process.Comp.FishingRod = fishingRod;
        process.Comp.FishingPool = fishingPool;
        process.Comp.User = user;

        process.Comp.Player = new Player(fishingRod.Comp.Size);
        process.Comp.Fish = new Fish(new MixedBehavior(), _random, _timing);

        process.Comp.LootProtoId = loot;
        process.Comp.StyleSheet = style;

        Dirty(process);

        Log.Debug($"Started new fishing process at {process}");
        return process;
    }

    public void Stop(Entity<CP14FishingProcessComponent> process)
    {
        var rod = GetRod(process);
        rod.Comp.Process = null;
        Dirty(rod);

        Del(process);
    }

    public Entity<CP14FishingProcessComponent> CreateProcess(EntityUid parent)
    {
        var entityUid = SpawnAttachedTo("MobHuman", Transform(parent).Coordinates);
        var ensureComponent = AddComp<CP14FishingProcessComponent>(entityUid);

        return (entityUid, ensureComponent);
    }

    private void UpdateReeling(Entity<CP14FishingProcessComponent> process,
        Entity<CP14FishingRodComponent> fishingRod,
        float frameTime)
    {
        if (fishingRod.Comp.Reeling)
        {
            process.Comp.Player.Velocity += fishingRod.Comp.Speed * frameTime;
            return;
        }

        process.Comp.Player.Velocity -= fishingRod.Comp.Gravity * frameTime;
    }

    private void UpdateVelocity(Entity<CP14FishingProcessComponent> process,
        Entity<CP14FishingRodComponent> fishingRod)
    {
        process.Comp.Player.Velocity *= fishingRod.Comp.Drag;
        process.Comp.Player.Velocity = Math.Clamp(process.Comp.Player.Velocity,
            fishingRod.Comp.MinVelocity,
            fishingRod.Comp.MaxVelocity);
    }

    private void UpdatePosition(Entity<CP14FishingProcessComponent> process,
        float frameTime)
    {
        process.Comp.Player.Position += process.Comp.Player.Velocity * frameTime;

        var halfSize = process.Comp.Player.HalfSize;
        process.Comp.Player.Position = Math.Clamp(process.Comp.Player.Position, halfSize, 1 - halfSize);
    }

    private void Update(Entity<CP14FishingProcessComponent> process, float frameTime)
    {
        var fishingRod = GetRod(process);

        UpdateReeling(process, fishingRod, frameTime);
        UpdateVelocity(process, fishingRod);
        UpdatePosition(process, frameTime);

        process.Comp.Fish.Update(frameTime);

        var collides = Collide(process.Comp.Fish, process.Comp.Player);

        var progressAdditive = collides ? 0.1f : -0.2f;
        process.Comp.Progress = Math.Clamp(process.Comp.Progress + progressAdditive * frameTime, 0, 1);

        Dirty(process);

        //if (process.Comp.Progress is 1 or 0)
       //     Finish(process, process.Comp.Progress is 1);
    }

    private void Finish(Entity<CP14FishingProcessComponent> process, bool success)
    {
        if (success)
        {
            Reward(process);
        }

        // Raising events before deletion
        var ev = new CP14FishingFinishedEvent(success);
        RaiseLocalEvent(process, ref ev, true);

        // Either way, we need to delete the process
        Stop(process);
    }

    private void Reward(Entity<CP14FishingProcessComponent> process)
    {
        var pool = GetPool(process);
        var rod = GetRod(process);

        var coordinates = Transform(pool).Coordinates;
        var targetCoordinates = Transform(process.Comp.User!.Value).Coordinates;

        var loot = Spawn(process.Comp.LootProtoId, coordinates);

        _throwing.TryThrow(loot, targetCoordinates, rod.Comp.ThrowPower);
    }
}
