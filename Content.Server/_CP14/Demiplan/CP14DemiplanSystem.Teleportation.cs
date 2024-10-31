using Content.Shared._CP14.Demiplan;
using Content.Shared._CP14.Demiplan.Components;
using Robust.Shared.Random;

namespace Content.Server._CP14.Demiplan;

public sealed partial class CP14DemiplanSystem
{
    private void InitTeleportation()
    {
        SubscribeLocalEvent<CP14DemiplanRiftOpenedComponent, CP14DemiplanPasswayUseDoAfter>(OnDemiplanPasswayDoAfter);

        SubscribeLocalEvent<CP14DemiplanRadiusTimedPasswayComponent, MapInitEvent>(RadiusMapInit);
    }

    private void UpdateTeleportation(float frameTime)
    {
        //Radius passway
        var query =
            EntityQueryEnumerator<CP14DemiplanRadiusTimedPasswayComponent, CP14DemiplanRiftComponent>();
        while (query.MoveNext(out var uid, out var passway, out var rift))
        {
            if (!passway.Enabled)
                continue;

            if (_timing.CurTime < passway.NextTimeTeleport)
                continue;

            passway.NextTimeTeleport = _timing.CurTime + passway.Delay;

            HashSet<EntityUid> teleportedEnts = new();
            var nearestEnts = _lookup.GetEntitiesInRange(uid, passway.Radius);
            foreach (var ent in nearestEnts)
            {
                if (!_mind.TryGetMind(ent, out var mindId, out var mind))
                    continue;

                teleportedEnts.Add(ent);
            }

            while (teleportedEnts.Count > passway.MaxEntities)
            {
                teleportedEnts.Remove(_random.Pick(teleportedEnts));
            }

            var map = Transform(uid).MapUid;
            if (TryComp<CP14DemiplanComponent>(map, out var demiplan))
            {
                if (!TryGetDemiplanExitPoint((map.Value, demiplan), out _))
                    break;

                foreach (var ent in teleportedEnts) //We in demiplan, tp OUT
                {
                    TryTeleportOutDemiplan((map.Value, demiplan), ent);
                }
            }
            else
            {
                if (rift.Demiplan is not null)
                {
                    if (!TryGetDemiplanEntryPoint(rift.Demiplan.Value, out _))
                        break;

                    foreach (var ent in teleportedEnts) //We out demiplan, tp IN
                    {
                        TryTeleportIntoDemiplan(rift.Demiplan.Value, ent);
                    }
                }
            }

            passway.Enabled = false;
        }
    }

    private void OnDemiplanPasswayDoAfter(Entity<CP14DemiplanRiftOpenedComponent> passWay, ref CP14DemiplanPasswayUseDoAfter args)
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
            if (TryComp<CP14DemiplanRiftComponent>(passWay, out var exitPoint) && exitPoint.Demiplan is not null)
            {
                used = TryTeleportIntoDemiplan(exitPoint.Demiplan.Value, args.User, passWay.Comp.DidItNude);
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


    private void RadiusMapInit(Entity<CP14DemiplanRadiusTimedPasswayComponent> passway, ref MapInitEvent args)
    {
        passway.Comp.NextTimeTeleport = _timing.CurTime + passway.Comp.Delay;
    }

    #region Teleportation

    public bool TryTeleportIntoDemiplan(Entity<CP14DemiplanComponent> demiplan, EntityUid target, bool nude = false)
    {
        if (!TryGetDemiplanEntryPoint(demiplan, out var entryPoint) || entryPoint is null)
        {
            Log.Error($"{target} cant get in demiplan {demiplan}: no active entry points!");
            return false;
        }

        var targetCoord = Transform(entryPoint.Value).Coordinates;
        _flash.Flash(target, null, null, 2f, 0.5f);
        _transform.SetCoordinates(target, targetCoord);
        _audio.PlayGlobal(demiplan.Comp.ArrivalSound, target);

        return true;
    }

    public bool TryTeleportOutDemiplan(Entity<CP14DemiplanComponent> demiplan, EntityUid target, bool nude = false)
    {
        if (!TryGetDemiplanExitPoint(demiplan, out var connection) || connection is null)
        {
            Log.Error($"{target} cant get out of demiplan {demiplan}: no active connections!");
            return false;
        }

        var targetCoord = Transform(connection.Value).Coordinates;
        _flash.Flash(target, null, null, 2f, 0.5f);
        _transform.SetCoordinates(target, targetCoord);
        _audio.PlayGlobal(demiplan.Comp.DepartureSound, target);

        return true;
    }

    #endregion
}
