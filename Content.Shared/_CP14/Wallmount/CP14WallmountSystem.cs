using Content.Shared.Tag;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.Wallmount;

public sealed class CP14WallmountSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;

    public static readonly ProtoId<TagPrototype>[] WallTags = {"Wall", "Window"};

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14WallmountedComponent, ComponentShutdown>(OnWallmountShutdown);
        SubscribeLocalEvent<CP14WallmountedComponent, AnchorStateChangedEvent>(OnWallmountAnchorChanged);
    }

    private void OnWallmountAnchorChanged(Entity<CP14WallmountedComponent> ent, ref AnchorStateChangedEvent args)
    {
        if (!args.Anchored)
            ClearWallmounts(ent);
    }

    private void OnWallmountShutdown(Entity<CP14WallmountedComponent> ent, ref ComponentShutdown args)
    {
        ClearWallmounts(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_net.IsClient)
            return;

        var query = EntityQueryEnumerator<CP14WallmountComponent>();
        while (query.MoveNext(out var uid, out var wallmount))
        {
            if (!wallmount.Initialized)
                continue;

            if (_timing.CurTime < wallmount.NextAttachTime)
                continue;

            if (wallmount.AttachAttempts <= 0)
            {
                QueueDel(uid);
                continue;
            }

            wallmount.NextAttachTime = _timing.CurTime + TimeSpan.FromSeconds(0.5f);
            wallmount.AttachAttempts--;

            if (TryAttachWallmount((uid, wallmount)))
            {
                RemComp<CP14WallmountComponent>(uid);
            }
        }
    }

    private void ClearWallmounts(Entity<CP14WallmountedComponent> ent)
    {
        foreach (var attached in ent.Comp.Attached)
        {
            QueueDel(attached);
        }

        ent.Comp.Attached.Clear();
    }

    private bool TryAttachWallmount(Entity<CP14WallmountComponent> wallmount)
    {
        var grid = Transform(wallmount).GridUid;
        if (grid == null || !TryComp<MapGridComponent>(grid, out var gridComp))
            return false;

        //Try found a wall in neighbour tile
        var offset = Transform(wallmount).LocalRotation.ToWorldVec().Normalized();
        var targetPos = new EntityCoordinates(grid.Value, Transform(wallmount).LocalPosition - offset);
        var anchored = _map.GetAnchoredEntities(grid.Value, gridComp, targetPos);

        bool hasParent = false;
        foreach (var entt in anchored)
        {
            if (!_tag.HasAnyTag(entt, WallTags))
                continue;

            EnsureComp<CP14WallmountedComponent>(entt, out var wallmounted);

            if (!wallmounted.Attached.Contains(wallmount))
                wallmounted.Attached.Add(wallmount);

            hasParent = true;
            break;
        }

        return hasParent;
    }
}
