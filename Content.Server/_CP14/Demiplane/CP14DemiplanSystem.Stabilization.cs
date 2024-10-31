using Content.Shared._CP14.Demiplane.Components;
using Content.Shared.Mobs.Systems;

namespace Content.Server._CP14.Demiplane;

public sealed partial class CP14DemiplaneSystem
{
    private readonly TimeSpan _checkFrequency = TimeSpan.FromSeconds(15f);
    private TimeSpan _nextCheckTime = TimeSpan.Zero;

    [Dependency] private readonly MobStateSystem _mobState = default!;

    private void InitStabilization()
    {
        _nextCheckTime = _timing.CurTime + _checkFrequency;
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

            if (stabilizer.RequireAlive && !(_mobState.IsAlive(uid) || _mobState.IsCritical(uid)))
                continue;

            if (stabilizedMaps.Contains(map.Value))
                continue;

            stabilizedMaps.Add(map.Value);
        }

        var query2 = EntityQueryEnumerator<CP14DemiplaneComponent, CP14DemiplaneDestroyWithoutStabilizationComponent>();
        while (query2.MoveNext(out var uid, out var demiplan, out var stabilization))
        {
            if (!stabilizedMaps.Contains(uid))
                QueueDel(uid);
        }
    }
}
