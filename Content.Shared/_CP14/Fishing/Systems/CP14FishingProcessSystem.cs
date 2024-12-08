using System.Diagnostics.CodeAnalysis;
using Content.Shared._CP14.Fishing.Components;
using Content.Shared._CP14.Fishing.Core;
using Content.Shared._CP14.Fishing.Core.Behaviors;
using Content.Shared._CP14.Fishing.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.Fishing.Systems;

public sealed class CP14FishingProcessSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly CP14FishingPoolSystem _pool = default!;

    private EntityQuery<CP14FishingRodComponent> _fishingRod;

    public override void Initialize()
    {
        base.Initialize();

        _fishingRod = GetEntityQuery<CP14FishingRodComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14FishingProcessComponent>();
        while (query.MoveNext(out var entityUid, out var processComponent))
        {
            Update((entityUid, processComponent), frameTime);
        }
    }

    private void Update(Entity<CP14FishingProcessComponent> process, float frameTime)
    {
        var fishingRod = GetRod(process);

        UpdateReeling(process, fishingRod, frameTime);
        UpdateVelocity(process, fishingRod);
        UpdatePosition(process, frameTime);

        process.Comp.Fish.Update(frameTime);

        var collides = Collide(process.Comp.Fish, process.Comp.Player);

        var progressAdditive = collides ? 0.05f : -0.1f;
        process.Comp.Progress = Math.Clamp(process.Comp.Progress + progressAdditive * frameTime, 0, 1);
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

    public void Finish(Entity<CP14FishingProcessComponent?> process)
    {

    }

    public Entity<CP14FishingProcessComponent> Start(
        Entity<CP14FishingRodComponent> fishingRod,
        Entity<CP14FishingPoolComponent> fishingPool,
        EntityUid user)
    {
        var process = CreateProcess();
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

        return process;
    }

    private Entity<CP14FishingProcessComponent> CreateProcess()
    {
        var entityUid = Spawn();
        var ensureComponent = EnsureComp<CP14FishingProcessComponent>(entityUid);

        return (entityUid, ensureComponent);
    }

    private Entity<CP14FishingRodComponent> GetRod(Entity<CP14FishingProcessComponent> process)
    {
        var entityUid = process.Comp.FishingRod!.Value;
        var component = _fishingRod.GetComponent(process.Comp.FishingRod!.Value);
        return (entityUid, component);
    }

    private CP14FishingProcessStyleSheetPrototype GetStyle(Entity<CP14FishingRodComponent> fishingRod)
    {
        if (_prototype.TryIndex(fishingRod.Comp.Style, out var style))
            return style;

        Log.Error($"Failed to retrieve fishing rod style, {fishingRod.Comp.Style} not found. Reverting to default style.");
        return _prototype.Index(CP14FishingRodComponent.DefaultStyle);
    }

    private static bool Collide(Fish fish, Player player)
    {
        var playerMin = player.Position - player.HalfSize;
        var playerMax = player.Position + player.HalfSize;
        return fish.Position >= playerMin && fish.Position <= playerMax;
    }
}
