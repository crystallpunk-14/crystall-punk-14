using Content.Server.Body.Systems;
using Content.Shared._CP14.Demiplane.Components;
using Content.Shared.Mobs.Systems;

namespace Content.Server._CP14.Demiplane;

public sealed partial class CP14DemiplaneSystem
{
    private readonly TimeSpan _checkFrequency = TimeSpan.FromSeconds(15f);
    private TimeSpan _nextCheckTime = TimeSpan.Zero;

    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly BodySystem _body = default!;

    private void InitStabilization()
    {
        _nextCheckTime = _timing.CurTime + _checkFrequency;

        SubscribeLocalEvent<CP14DemiplaneDestroyWithoutStabilizationComponent, MapInitEvent>(OnStabilizationMapInit);
    }

    private void OnStabilizationMapInit(Entity<CP14DemiplaneDestroyWithoutStabilizationComponent> ent,
        ref MapInitEvent args)
    {
        ent.Comp.EndProtectionTime = _timing.CurTime + ent.Comp.ProtectedSpawnTime;
    }

    private void UpdateStabilization(float frameTime)
    {
        if (_timing.CurTime < _nextCheckTime)
            return;

        _nextCheckTime = _timing.CurTime + _checkFrequency;

        HashSet<EntityUid> stabilizedMaps = new();

        var query = EntityQueryEnumerator<CP14DemiplaneStabilizerComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var stabilizer, out var transform))
        {
            var map = transform.MapUid;

            if (map is null)
                continue;

            if (!stabilizer.Enabled)
                continue;

            if (stabilizer.RequireAlive && !(_mobState.IsAlive(uid) || _mobState.IsCritical(uid)))
                continue;

            if (stabilizedMaps.Contains(map.Value))
                continue;

            stabilizedMaps.Add(map.Value);
        }

        var query2 = EntityQueryEnumerator<CP14DemiplaneComponent, CP14DemiplaneDestroyWithoutStabilizationComponent>();
        while (query2.MoveNext(out var uid, out var demiplane, out var stabilization))
        {
            if (_timing.CurTime < stabilization.EndProtectionTime)
                continue;

            if (!stabilizedMaps.Contains(uid))
                DeleteDemiplane((uid, demiplane));
        }
    }

    public void DeleteAllDemiplanes(bool safe = true)
    {
        var query = EntityQueryEnumerator<CP14DemiplaneComponent>();

        while (query.MoveNext(out var uid, out var demiplane))
        {
            DeleteDemiplane((uid, demiplane), safe);
        }
    }

    public void DeleteDemiplane(Entity<CP14DemiplaneComponent> demiplane, bool safe = false)
    {
        var query = EntityQueryEnumerator<CP14DemiplaneStabilizerComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var stabilizer, out var xform))
        {
            if (!stabilizer.Enabled)
                continue;

            if (TryTeleportOutDemiplane(demiplane, uid))
            {
                if (!safe)
                {
                    var ev = new CP14DemiplaneUnsafeExit();
                    RaiseLocalEvent(uid, ev);

                    _body.GibBody(uid);
                }
            }
        }

        QueueDel(demiplane);
    }
}

public sealed class CP14DemiplaneUnsafeExit : EntityEventArgs
{

}
