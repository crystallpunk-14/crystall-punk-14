using Content.Shared._CP14.Demiplan;
using Content.Shared._CP14.Demiplan.Components;
using Content.Shared.Movement.Pulling.Components;
using Robust.Shared.Random;

namespace Content.Server._CP14.Demiplan;

public sealed partial class CP14DemiplanSystem
{
    private void InitTeleportation()
    {
        SubscribeLocalEvent<CP14DemiplanPasswayComponent, CP14DemiplanPasswayUseDoAfter>(OnDemiplanPasswayDoAfter);
    }

    private void OnDemiplanPasswayDoAfter(Entity<CP14DemiplanPasswayComponent> passWay, ref CP14DemiplanPasswayUseDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        var used = false;
        var map = Transform(passWay).MapUid;
        if (TryComp<CP14DemiplanComponent>(map, out var demiplan))
        {
            used = TryTeleportOutDemiplan((map.Value, demiplan), args.User, passWay.Comp.DidItNude);
        }
        else
        {
            if (TryComp<CP14DemiplanConnectionComponent>(passWay, out var connection) && connection.Link is not null)
            {
                used = TryTeleportIntoDemiplan(connection.Link.Value, args.User, passWay.Comp.DidItNude);
            }
        }

        if (passWay.Comp.MaxUse > 0 && used)
        {
            passWay.Comp.MaxUse--;
            if (passWay.Comp.MaxUse == 0)
                QueueDel(passWay);
        }

        args.Handled = true;
    }

    private bool TryTeleportIntoDemiplan(Entity<CP14DemiplanComponent> demiplan, EntityUid target, bool nude = false)
    {
        if (demiplan.Comp.EntryPoints.Count == 0)
        {
            Log.Error($"{target} cant get in demiplan {demiplan}: no active entry points!");
            return false;
        }

        var targetPos = _random.Pick(demiplan.Comp.EntryPoints);
        DemiplanTeleport(target, targetPos, nude);
        return true;
    }

    private bool TryTeleportOutDemiplan(Entity<CP14DemiplanComponent> demiplan, EntityUid target, bool nude = false)
    {
        if (demiplan.Comp.Connections.Count == 0)
        {
            Log.Error($"{target} cant get out of demiplan {demiplan}: no active connections!");
            return false;
        }

        var targetPos = _random.Pick(demiplan.Comp.Connections);

        DemiplanTeleport(target, targetPos, nude);
        return true;
    }

    private void DemiplanTeleport(EntityUid target, EntityUid destination, bool nude = false)
    {
        HashSet<EntityUid> teleportEnts = new();
        teleportEnts.Add(target);

        if (TryComp<PullerComponent>(target, out var puller))
        {
            if (puller.Pulling is not null)
                teleportEnts.Add(puller.Pulling.Value);
        }

        var targetCoord = Transform(destination).Coordinates;
        foreach (var ent in teleportEnts)
        {
            //todo: Effect spawn
            _transform.SetCoordinates(ent, targetCoord);
            _throwing.TryThrow(ent, _random.NextAngle().ToWorldVec());
        }
    }
}
