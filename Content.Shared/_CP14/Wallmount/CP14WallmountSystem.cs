using Content.Shared.Tag;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Wallmount;

public sealed class CP14WallmountSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    public static readonly ProtoId<TagPrototype>[] WallTags = {"Wall", "Window"};

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14WallmountComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<CP14WallmountComponent> ent, ref MapInitEvent args)
    {
        var grid = Transform(ent).GridUid;
        if (grid == null || !TryComp<MapGridComponent>(grid, out var gridComp))
            return;

        //Try found a wall in neighbour tile
        var offset = Transform(ent).LocalRotation.ToWorldVec();
        var targetPos = new EntityCoordinates(grid.Value,Transform(ent).LocalPosition - offset);
        var anchored = _map.GetAnchoredEntities(grid.Value, gridComp, targetPos);

        bool hasParent = false;
        foreach (var entt in anchored)
        {
            if (!_tag.HasAnyTag(entt, WallTags))
                continue;

            _transform.SetParent(ent, entt);
            hasParent = true;
            break;
        }
        if (!hasParent)
            QueueDel(ent);
    }
}
