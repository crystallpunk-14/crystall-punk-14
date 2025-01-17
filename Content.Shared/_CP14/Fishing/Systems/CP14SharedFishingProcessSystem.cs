using System.Diagnostics.CodeAnalysis;
using Content.Shared._CP14.Fishing.Components;
using Content.Shared._CP14.Fishing.Core;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.Fishing.Systems;

public abstract partial class CP14SharedFishingProcessSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    protected EntityQuery<CP14FishingRodComponent> FishingRod;
    protected EntityQuery<CP14FishingPoolComponent> FishingPool;

    private int _frames;

    public override void Initialize()
    {
        base.Initialize();

        FishingRod = GetEntityQuery<CP14FishingRodComponent>();
        FishingPool = GetEntityQuery<CP14FishingPoolComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14FishingProcessComponent>();
        while (query.MoveNext(out var entityUid, out var processComponent))
        {
            Update((entityUid, processComponent), frameTime);
        }

        Log.Debug($"Frame {_frames++} FrameTime: {frameTime}");
    }

    private void UpdateReeling(Entity<CP14FishingProcessComponent> process,
        Entity<CP14FishingRodComponent> fishingRod,
        float frameTime)
    {
        if (process.Comp.Player is not { } player)
            return;

        if (fishingRod.Comp.Reeling)
        {
            player.Velocity += fishingRod.Comp.Speed * frameTime;
            return;
        }

        player.Velocity -= fishingRod.Comp.Gravity * frameTime;
    }

    private void UpdateVelocity(Entity<CP14FishingProcessComponent> process,
        Entity<CP14FishingRodComponent> fishingRod)
    {
        if (process.Comp.Player is not { } player)
            return;

        player.Velocity *= fishingRod.Comp.Drag;
        player.Velocity = Math.Clamp(player.Velocity,
            fishingRod.Comp.MinVelocity,
            fishingRod.Comp.MaxVelocity);
    }

    private void UpdatePosition(Entity<CP14FishingProcessComponent> process,
        float frameTime)
    {
        if (process.Comp.Player is not { } player)
            return;

        player.Position += process.Comp.Player.Velocity * frameTime;

        var halfSize = process.Comp.Player.HalfSize;
        process.Comp.Player.Position = Math.Clamp(process.Comp.Player.Position, halfSize, 1 - halfSize);
    }

    public void Update(Entity<CP14FishingProcessComponent> process, float frameTime)
    {
        if (process.Comp.Player is not { } player)
            return;

        var fishingRod = GetRod(process);

        UpdateReeling(process, fishingRod, frameTime);
        UpdateVelocity(process, fishingRod);
        UpdatePosition(process, frameTime);

        if (process.Comp.Fish is { } fish)
        {
            fish.Update(frameTime);
            fish.UpdateSpeed(_timing);

            var collides = Collide(fish, player);

            var progressAdditive = collides ? 0.1f : -0.2f;
            process.Comp.Progress = Math.Clamp(process.Comp.Progress + progressAdditive * frameTime, 0, 1);
        }

        UpdateDirty(process);

        if (process.Comp.Progress is 1 or 0)
            Finish(process, process.Comp.Progress is 1);
    }

    public virtual void FishPreUpdate(Entity<CP14FishingProcessComponent> process, Fish fish, float frameTime)
    {
    }

    public virtual void UpdateDirty(Entity<CP14FishingProcessComponent> process)
    {
    }

    public virtual void Finish(Entity<CP14FishingProcessComponent> process, bool success)
    {
    }

    public void Stop(Entity<CP14FishingProcessComponent> process)
    {
        var rod = GetRod(process);
        rod.Comp.Process = null;
        Dirty(rod);

        Del(process);
    }

    public virtual void Reward(Entity<CP14FishingProcessComponent> process)
    {
    }

    public bool TryGetByUser(EntityUid userEntityUid, [NotNullWhen(true)] out Entity<CP14FishingProcessComponent>? process)
    {
        process = null;

        var query = EntityQueryEnumerator<CP14FishingProcessComponent>();
        while (query.MoveNext(out var entityUid, out var processComponent))
        {
            if (processComponent.User != userEntityUid)
                continue;

            process = (entityUid, processComponent);
            return true;
        }

        return false;
    }
}
