using Content.Server._CP14.Demiplane;
using Content.Server.Mind;
using Content.Shared._CP14.Demiplane;
using Content.Shared._CP14.Demiplane.Components;
using Content.Shared._CP14.DemiplaneTraveling;
using Content.Shared.Ghost;
using Content.Shared.Movement.Pulling.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.DemiplaneTraveling;

public sealed partial class CP14DemiplaneTravelingSystem : EntitySystem
{
    [Dependency] private readonly CP14DemiplaneSystem _demiplan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14DemiplaneRadiusTimedPasswayComponent, MapInitEvent>(RadiusMapInit);
        SubscribeLocalEvent<CP14DemiplaneRiftOpenedComponent, CP14DemiplanPasswayUseDoAfter>(OnOpenRiftInteractDoAfter);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        //Radius passway
        var query = EntityQueryEnumerator<CP14DemiplaneRadiusTimedPasswayComponent, CP14DemiplaneRiftComponent>();
        while (query.MoveNext(out var uid, out var passway, out var rift))
        {
            if (_timing.CurTime < passway.NextTimeTeleport)
                continue;

            passway.NextTimeTeleport = _timing.CurTime + passway.Delay;

            HashSet<EntityUid> teleportedEnts = new();
            var nearestEnts = _lookup.GetEntitiesInRange(uid, passway.Radius);
            foreach (var ent in nearestEnts)
            {
                if (HasComp<GhostComponent>(ent))
                    continue;

                if (!_mind.TryGetMind(ent, out var mindId, out var mind))
                    continue;

                teleportedEnts.Add(ent);
            }

            while (teleportedEnts.Count > passway.MaxEntities)
            {
                teleportedEnts.Remove(_random.Pick(teleportedEnts));
            }

            var map = Transform(uid).MapUid;
            if (TryComp<CP14DemiplaneComponent>(map, out var demiplan))
            {
                if (!_demiplan.TryGetDemiplanExitPoint((map.Value, demiplan), out _))
                    break;

                foreach (var ent in teleportedEnts) //We in demiplan, tp OUT
                {
                    if (TryComp<PullerComponent>(ent, out var puller))
                        _demiplan.TryTeleportOutDemiplane((map.Value, demiplan), puller.Pulling);

                    _demiplan.TryTeleportOutDemiplane((map.Value, demiplan), ent);
                }
            }
            else
            {
                if (rift.Demiplan is not null)
                {
                    if (!_demiplan.TryGetDemiplanEntryPoint(rift.Demiplan.Value, out _))
                        break;

                    foreach (var ent in teleportedEnts) //We out demiplan, tp IN
                    {
                        if (TryComp<PullerComponent>(ent, out var puller))
                            _demiplan.TryTeleportIntoDemiplane(rift.Demiplan.Value, puller.Pulling);

                        _demiplan.TryTeleportIntoDemiplane(rift.Demiplan.Value, ent);
                    }
                }
            }
            QueueDel(uid);
        }
    }

    private void RadiusMapInit(Entity<CP14DemiplaneRadiusTimedPasswayComponent> radiusPassWay, ref MapInitEvent args)
    {
        radiusPassWay.Comp.NextTimeTeleport = _timing.CurTime + radiusPassWay.Comp.Delay;
    }

    private void OnOpenRiftInteractDoAfter(Entity<CP14DemiplaneRiftOpenedComponent> passWay, ref CP14DemiplanPasswayUseDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        var used = false;
        var map = Transform(passWay).MapUid;
        if (TryComp<CP14DemiplaneComponent>(map, out var demiplan))
        {
            if (TryComp<PullerComponent>(args.User, out var puller))
                _demiplan.TryTeleportOutDemiplane((map.Value, demiplan), puller.Pulling);

            used = _demiplan.TryTeleportOutDemiplane((map.Value, demiplan), args.User);
        }
        else
        {
            if (TryComp<CP14DemiplaneRiftComponent>(passWay, out var exitPoint) && exitPoint.Demiplan is not null)
            {
                if (TryComp<PullerComponent>(args.User, out var puller))
                    _demiplan.TryTeleportIntoDemiplane(exitPoint.Demiplan.Value, puller.Pulling);

                used = _demiplan.TryTeleportIntoDemiplane(exitPoint.Demiplan.Value, args.User);
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
}
