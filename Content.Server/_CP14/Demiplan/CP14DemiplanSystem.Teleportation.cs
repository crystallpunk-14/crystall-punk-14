using Content.Shared._CP14.Demiplan;
using Content.Shared._CP14.Demiplan.Components;
using Content.Shared.Movement.Pulling.Components;
using Robust.Shared.Random;

namespace Content.Server._CP14.Demiplan;

public sealed partial class CP14DemiplanSystem
{
    private void InitTeleportation()
    {
        SubscribeLocalEvent<CP14DemiplanExitComponent, MapInitEvent>(OnDemiplanExitMapInit);
        SubscribeLocalEvent<CP14DemiplanExitComponent, CP14DemiplanExitDoAfter>(OnDemiplanExitDoAfter);
    }

    private void OnDemiplanExitMapInit(Entity<CP14DemiplanExitComponent> exit, ref MapInitEvent args)
    {
        var map = Transform(exit).MapUid;
        if (!TryComp<CP14DemiplanComponent>(map, out var demiplan))
        {
            QueueDel(exit);
        }
        else
        {
            exit.Comp.Demiplan = (map.Value, demiplan);
        }
    }

    private void OnDemiplanExitDoAfter(Entity<CP14DemiplanExitComponent> exit, ref CP14DemiplanExitDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;


        if (exit.Comp.Demiplan is null)
            return;

        TeleportOutDemiplan(exit.Comp.Demiplan.Value, args.User, exit.Comp.DidItNude);

        args.Handled = true;
    }

    private bool TryTeleportIntoDemiplan(Entity<CP14DemiplanComponent> demiplan, EntityUid target)
    {
        HashSet<EntityUid> teleportEnts = new();

        teleportEnts.Add(target);

        if (TryComp<PullerComponent>(target, out var puller))
        {
            if (puller.Pulling is not null)
                teleportEnts.Add(puller.Pulling.Value);
        }

        if (demiplan.Comp.EntryPoints.Count == 0)
        {
            Log.Error($"{target} cant get in demiplan {demiplan}: no active entry points!");
            return false;
        }

        var targetPos = _random.Pick(demiplan.Comp.EntryPoints);
        var targetCoord = Transform(targetPos).Coordinates;
        foreach (var ent in teleportEnts)
        {
            //todo: Effect spawn
            _transform.SetCoordinates(ent, targetCoord);
            _throwing.TryThrow(ent, _random.NextAngle().ToWorldVec(), 2);
        }

        return true;
    }

    private void TeleportOutDemiplan(Entity<CP14DemiplanComponent> demiplan, EntityUid target, bool nude)
    {
        HashSet<EntityUid> teleportEnts = new();

        teleportEnts.Add(target);

        if (TryComp<PullerComponent>(target, out var puller))
        {
            if (puller.Pulling is not null)
                teleportEnts.Add(puller.Pulling.Value);
        }

        if (demiplan.Comp.Connections.Count == 0)
        {
            Log.Error($"{target} cant get out of demiplan {demiplan}: no active connections!");
            return;
        }

        var targetPos = _random.Pick(demiplan.Comp.Connections);
        var targetCoord = Transform(targetPos).Coordinates;
        foreach (var ent in teleportEnts)
        {
            //todo: Effect spawn
            _transform.SetCoordinates(ent, targetCoord);
            _throwing.TryThrow(ent, _random.NextAngle().ToWorldVec(), 2);
        }
    }
}
