using Content.Shared._CP14.ZLevel;
using Content.Shared.Actions;
using Robust.Shared.Map;

namespace Content.Server._CP14.ZLevels.EntitySystems;

public sealed partial class CP14StationZLevelsSystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    private void InitActions()
    {
        SubscribeLocalEvent<CP14ZLevelMoverComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CP14ZLevelMoverComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<CP14ZLevelMoverComponent, CP14ZLevelActionUp>(OnZLevelUpGhost);
        SubscribeLocalEvent<CP14ZLevelMoverComponent, CP14ZLevelActionDown>(OnZLevelDownGhost);
    }

    private void OnMapInit(Entity<CP14ZLevelMoverComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ref ent.Comp.CP14ZLevelUpActionEntity, ent.Comp.UpActionProto);
        _actions.AddAction(ent, ref ent.Comp.CP14ZLevelDownActionEntity, ent.Comp.DownActionProto);
    }

    private void OnRemove(Entity<CP14ZLevelMoverComponent> ent, ref ComponentRemove args)
    {
        _actions.RemoveAction(ent.Comp.CP14ZLevelUpActionEntity);
        _actions.RemoveAction(ent.Comp.CP14ZLevelDownActionEntity);
    }

    private void OnZLevelDownGhost(Entity<CP14ZLevelMoverComponent> ent, ref CP14ZLevelActionDown args)
    {
        if (args.Handled)
            return;

        ZLevelMove(ent, -1);

        args.Handled = true;
    }

    private void OnZLevelUpGhost(Entity<CP14ZLevelMoverComponent> ent, ref CP14ZLevelActionUp args)
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
