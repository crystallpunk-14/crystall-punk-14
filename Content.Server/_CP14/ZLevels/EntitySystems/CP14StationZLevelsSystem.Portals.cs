using Content.Server._CP14.ZLevels.Components;
using Content.Server.GameTicking.Events;
using Content.Shared.Teleportation.Systems;
using Robust.Shared.Map;

namespace Content.Server._CP14.ZLevels.EntitySystems;

public sealed partial class CP14StationZLevelsSystem
{
    [Dependency] private readonly LinkedEntitySystem _linkedEntity = default!;
    private void InitializePortals()
    {
        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);
        SubscribeLocalEvent<CP14ZLevelAutoPortalComponent, MapInitEvent>(OnPortalMapInit);
    }

    private void OnRoundStart(RoundStartingEvent ev)
    {
        var query = EntityQueryEnumerator<CP14ZLevelAutoPortalComponent>();
        while (query.MoveNext(out var uid, out var portal))
        {
            InitPortal((uid, portal));
        }
    }

    private void OnPortalMapInit(Entity<CP14ZLevelAutoPortalComponent> autoPortal, ref MapInitEvent args)
    {
        InitPortal(autoPortal);
    }

    private void InitPortal(Entity<CP14ZLevelAutoPortalComponent> autoPortal)
    {
        var mapId = Transform(autoPortal).MapUid;
        if (mapId is null)
            return;

        var offsetMap = GetMapOffset(mapId.Value, autoPortal.Comp.ZLevelOffset);

        if (offsetMap is null)
            return;

        var currentWorldPos = _transform.GetWorldPosition(autoPortal);
        var targetMapPos = new MapCoordinates(currentWorldPos, offsetMap.Value);

        var otherSidePortal = Spawn(autoPortal.Comp.OtherSideProto, targetMapPos);

        _transform.SetWorldRotation(otherSidePortal, _transform.GetWorldRotation(autoPortal));
        if (_linkedEntity.TryLink(autoPortal, otherSidePortal, true))
            RemComp<CP14ZLevelAutoPortalComponent>(autoPortal);
    }
}
