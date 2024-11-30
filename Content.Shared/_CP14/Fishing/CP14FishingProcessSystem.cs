using System.Diagnostics.CodeAnalysis;
using Content.Shared._CP14.Fishing.FishingPool;
using Content.Shared._CP14.Fishing.FishingRod;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Fishing;

public sealed class CP14FishingProcessSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

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
        var fishingRodComponent = _fishingRod.GetComponent(process.Comp.FishingRod!.Value);

        if (fishingRodComponent.Reeling)
            process.Comp.Player.Position += fishingRodComponent.Speed * frameTime;
        else
            process.Comp.Player.Position -= process.Comp.Gravity * frameTime;

        // Simple collision with board
        // TODO: bouncing
        process.Comp.Player.Clamp(process.Comp.Size);
        process.Comp.Loot.Clamp(process.Comp.Size);
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

    public Entity<CP14FishingProcessComponent> Start(ProtoId<CP14FishingProcessPrototype> prototypeId,
        Entity<CP14FishingRodComponent> fishingRod,
        Entity<CP14FishingPoolComponent> fishingPool,
        EntityUid user)
    {
        var prototype = _prototype.Index(prototypeId);
        return Start(prototype, fishingRod, fishingPool, user);
    }

    public Entity<CP14FishingProcessComponent> Start(CP14FishingProcessPrototype prototype,
        Entity<CP14FishingRodComponent> fishingRod,
        Entity<CP14FishingPoolComponent> fishingPool,
        EntityUid user)
    {
        var entityUid = Spawn();
        var ensureComponent = EnsureComp<CP14FishingProcessComponent>(entityUid);

        ensureComponent.FishingRod = fishingRod;
        ensureComponent.FishingPool = fishingPool;
        ensureComponent.User = user;

        ensureComponent.Size = prototype.Size;
        ensureComponent.Gravity = prototype.Gravity;

        ensureComponent.Player = new CP14FishingProcessComponent.Box(prototype.PlayerSize);
        ensureComponent.Loot = new CP14FishingProcessComponent.Box(prototype.LootSize);

        ensureComponent.LootProtoId = prototype.LootProtoId;
        ensureComponent.StyleSheet = _prototype.Index<CP14FishingProcessStyleSheetPrototype>("Default");

        Dirty(entityUid, ensureComponent);

        return (entityUid, ensureComponent);
    }
}
