using Content.Server._CP14.Demiplane;
using Content.Server._CP14.RoundEnd;
using Content.Server.Interaction;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Shared._CP14.Demiplane;
using Content.Shared._CP14.Demiplane.Components;
using Content.Shared._CP14.DemiplaneTraveling;
using Content.Shared.Ghost;
using Content.Shared.Movement.Pulling.Components;
using Robust.Shared.Audio.Systems;
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
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly InteractionSystem _interaction = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14DemiplaneRadiusTimedPasswayComponent, MapInitEvent>(RadiusMapInit);
        SubscribeLocalEvent<CP14MonolithTimedPasswayComponent, MapInitEvent>(MonolithMapInit);
        SubscribeLocalEvent<CP14DemiplaneRiftOpenedComponent, CP14DemiplanPasswayUseDoAfter>(OnOpenRiftInteractDoAfter);
    }

    // !!!SHITCODE WARNING!!!
    // This whole module is saturated with shieldcode, code duplication and other delights. Why? Because.
    //TODO: Refactor this shitcode
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        DemiplaneTeleportUpdate();

        var query = EntityQueryEnumerator<CP14MonolithTimedPasswayComponent>();
        while (query.MoveNext(out var uid, out var passWay))
        {
            if (_timing.CurTime < passWay.NextTimeTeleport)
                continue;

            passWay.NextTimeTeleport = _timing.CurTime + passWay.Delay;

            //Get all teleporting entities
            HashSet<EntityUid> teleportedEnts = new();
            var nearestEnts = _lookup.GetEntitiesInRange(uid, passWay.Radius);
            foreach (var ent in nearestEnts)
            {
                if (HasComp<GhostComponent>(ent))
                    continue;

                if (!_mind.TryGetMind(ent, out var mindId, out var mind))
                    continue;

                if (!_interaction.InRangeUnobstructed(ent, uid))
                    continue;

                teleportedEnts.Add(ent);
            }

            while (teleportedEnts.Count > passWay.MaxEntities)
            {
                teleportedEnts.Remove(_random.Pick(teleportedEnts));
            }

            //Aaaand teleport it
            var monoliths = EntityQueryEnumerator<CP14MagicContainerRoundFinisherComponent>();
            while (monoliths.MoveNext(out var monolithUid, out var monolith))
            {
                var coord = Transform(monolithUid).Coordinates;

                //Shitcode select first one
                foreach (var ent in teleportedEnts)
                {
                    if (TryComp<PullerComponent>(ent, out var puller))
                        _demiplan.TeleportEntityToCoordinate(puller.Pulling, coord);

                    _demiplan.TeleportEntityToCoordinate(ent, coord);
                    _audio.PlayPvs(passWay.ArrivalSound, ent);
                }
                break;
            }

            _audio.PlayPvs(passWay.DepartureSound, Transform(uid).Coordinates);
            QueueDel(uid);
        }
    }

    private void DemiplaneTeleportUpdate()
    {
        //Radius passway
        var query = EntityQueryEnumerator<CP14DemiplaneRadiusTimedPasswayComponent, CP14DemiplaneRiftComponent>();
        while (query.MoveNext(out var uid, out var passWay, out var rift))
        {
            if (_timing.CurTime < passWay.NextTimeTeleport)
                continue;

            passWay.NextTimeTeleport = _timing.CurTime + passWay.Delay;

            //Get all teleporting entities
            HashSet<EntityUid> teleportedEnts = new();
            var nearestEnts = _lookup.GetEntitiesInRange(uid, passWay.Radius);
            foreach (var ent in nearestEnts)
            {
                if (HasComp<GhostComponent>(ent))
                    continue;

                if (!_mind.TryGetMind(ent, out var mindId, out var mind))
                    continue;

                if (!_interaction.InRangeUnobstructed(ent, uid))
                    continue;

                teleportedEnts.Add(ent);
            }

            while (teleportedEnts.Count > passWay.MaxEntities)
            {
                teleportedEnts.Remove(_random.Pick(teleportedEnts));
            }

            //Aaaand teleport it
            var map = Transform(uid).MapUid;
            if (TryComp<CP14DemiplaneComponent>(map, out var demiplan))
            {
                if (!_demiplan.TryGetDemiplaneExitPoint((map.Value, demiplan), out _))
                    break;

                foreach (var ent in teleportedEnts) //We in demiplan, tp OUT
                {
                    if (TryComp<PullerComponent>(ent, out var puller))
                        _demiplan.TryTeleportOutDemiplane((map.Value, demiplan), puller.Pulling);

                    _demiplan.TryTeleportOutDemiplane((map.Value, demiplan), ent);
                    _audio.PlayPvs(passWay.ArrivalSound, ent);
                }
            }
            else
            {
                if (rift.Demiplane is not null &&
                    TryComp<CP14DemiplaneComponent>(rift.Demiplane.Value, out var riftDemiplane))
                {
                    if (!_demiplan.TryGetDemiplaneEntryPoint((rift.Demiplane.Value, riftDemiplane), out _))
                        break;

                    foreach (var ent in teleportedEnts) //We out demiplan, tp IN
                    {
                        if (TryComp<PullerComponent>(ent, out var puller))
                            _demiplan.TryTeleportIntoDemiplane((rift.Demiplane.Value, riftDemiplane), puller.Pulling);

                        _demiplan.TryTeleportIntoDemiplane((rift.Demiplane.Value, riftDemiplane), ent);
                        _audio.PlayPvs(passWay.ArrivalSound, ent);
                    }
                }
            }

            _audio.PlayPvs(passWay.DepartureSound, Transform(uid).Coordinates);
            QueueDel(uid);
        }
    }

    private void RadiusMapInit(Entity<CP14DemiplaneRadiusTimedPasswayComponent> radiusPassWay, ref MapInitEvent args)
    {
        radiusPassWay.Comp.NextTimeTeleport = _timing.CurTime + radiusPassWay.Comp.Delay;
    }

    private void MonolithMapInit(Entity<CP14MonolithTimedPasswayComponent> radiusPassWay, ref MapInitEvent args)
    {
        radiusPassWay.Comp.NextTimeTeleport = _timing.CurTime + radiusPassWay.Comp.Delay;
    }

    private void OnOpenRiftInteractDoAfter(Entity<CP14DemiplaneRiftOpenedComponent> passWay,
        ref CP14DemiplanPasswayUseDoAfter args)
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
            if (TryComp<CP14DemiplaneRiftComponent>(passWay, out var exitPoint) && exitPoint.Demiplane is not null &&
                TryComp<CP14DemiplaneComponent>(exitPoint.Demiplane.Value, out var exitDemiplane))
            {
                if (TryComp<PullerComponent>(args.User, out var puller))
                    _demiplan.TryTeleportIntoDemiplane((exitPoint.Demiplane.Value, exitDemiplane), puller.Pulling);

                used = _demiplan.TryTeleportIntoDemiplane((exitPoint.Demiplane.Value, exitDemiplane), args.User);
            }
        }

        if (used)
        {
            _audio.PlayPvs(passWay.Comp.DepartureSound, Transform(passWay).Coordinates);
            _audio.PlayPvs(passWay.Comp.ArrivalSound, args.User);
            if (passWay.Comp.MaxUse > 0)
            {
                passWay.Comp.MaxUse--;
                if (passWay.Comp.MaxUse == 0)
                    QueueDel(passWay);
            }
        }

        args.Handled = true;
    }
}
