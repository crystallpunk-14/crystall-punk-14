using System.Numerics;
using Content.Shared._CP14.ZLevel;
using Content.Shared.Ghost;
using Robust.Shared.Map;

namespace Content.Server._CP14.ZLevels.EntitySystems;

public sealed partial class CP14StationZLevelsSystem
{
    private void InitActions()
    {
        SubscribeLocalEvent<GhostComponent, CP14ZLevelActionUp>(OnZLevelUp);
        SubscribeLocalEvent<GhostComponent, CP14ZLevelActionDown>(OnZLevelDown);
    }

    private void OnZLevelDown(Entity<GhostComponent> ent, ref CP14ZLevelActionDown args)
    {
        if (args.Handled)
            return;

        ZLevelMove(ent, -1);

        args.Handled = true;
    }

    private void OnZLevelUp(Entity<GhostComponent> ent, ref CP14ZLevelActionUp args)
    {
        if (args.Handled)
            return;

        ZLevelMove(ent, 1);

        args.Handled = true;
    }

    private void ZLevelMove(EntityUid ent, int offset)
    {
        var xform = Transform(ent);
        var map = xform.MapUid;

        if (map is null)
            return;

        var targetMap = GetMapOffset(map.Value, offset);

        if (targetMap is null)
            return;

        _transform.SetMapCoordinates(ent, new MapCoordinates(_transform.GetWorldPosition(ent), targetMap.Value));
    }
}
