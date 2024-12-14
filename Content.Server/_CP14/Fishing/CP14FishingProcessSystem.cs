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

    /*
    private readonly TimeSpan _dirtyDelay = TimeSpan.FromTicks(10000000000000);
    private TimeSpan _dirtyDelayTime;
    */

    public override void Update(float frameTime)
    {
        // DON'T CALL BASE METHOD!!!

        var query = EntityQueryEnumerator<CP14FishingProcessComponent>();
        while (query.MoveNext(out var entityUid, out var processComponent))
        {
            Update((entityUid, processComponent), frameTime * 2);
        }
    }

    public override void FishPreUpdate(Entity<CP14FishingProcessComponent> process, Fish fish, float frameTime)
    {
        base.FishPreUpdate(process, fish, frameTime);

        fish.UpdateSpeed(_random, _timing);
        Dirty(process);
    }

    public override void UpdateDirty(Entity<CP14FishingProcessComponent> process)
    {
        base.UpdateDirty(process);

        /*
        if (_timing.CurTime < _dirtyDelayTime)
            return;

        _dirtyDelayTime = _timing.CurTime + _dirtyDelay;
        Dirty(process);
        */
    }

    public override void Finish(Entity<CP14FishingProcessComponent> process, bool success)
    {
        base.Finish(process, success);

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

    public override void Reward(Entity<CP14FishingProcessComponent> process)
    {
        base.Reward(process);

        var pool = GetPool(process);
        var rod = GetRod(process);

        var coordinates = Transform(pool).Coordinates;
        var targetCoordinates = Transform(process.Comp.User!.Value).Coordinates;

        var loot = Spawn(process.Comp.LootProtoId, coordinates);

        _throwing.TryThrow(loot, targetCoordinates, rod.Comp.ThrowPower);
    }

    public Entity<CP14FishingProcessComponent> Start(
        Entity<CP14FishingRodComponent> fishingRod,
        Entity<CP14FishingPoolComponent> fishingPool,
        EntityUid user)
    {
        var process = CreateProcess(fishingRod.Owner);
        var loot = _pool.GetLootPrototypeId(fishingPool);
        var style = GetStyle(fishingRod);

        // Save entities
        process.Comp.FishingRod = fishingRod;
        process.Comp.FishingPool = fishingPool;
        process.Comp.User = user;

        process.Comp.Player = new Player(fishingRod.Comp.Size);
        process.Comp.Fish = new Fish(new MixedBehavior(), _timing.CurTime + TimeSpan.FromSeconds(0.5f));

        process.Comp.LootProtoId = loot;
        process.Comp.StyleSheet = style;

        Dirty(process);

        Log.Debug($"Started new fishing process at {process}");
        return process;
    }

    public Entity<CP14FishingProcessComponent> CreateProcess(EntityUid parent)
    {
        var entityUid = SpawnAttachedTo(null, Transform(parent).Coordinates);
        var ensureComponent = AddComp<CP14FishingProcessComponent>(entityUid);

        return (entityUid, ensureComponent);
    }
}
