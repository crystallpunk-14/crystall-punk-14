using Content.Server.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Ghost;
using Content.Shared.Mind.Components;
using Robust.Shared;
using Robust.Shared.Timing;

namespace Content.Server._CP14.PVS;

public sealed partial class CP14HelperPvsSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;

    private static readonly TimeSpan UpdateFrequency = TimeSpan.FromSeconds(5f);

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14OutPVSDespawnComponent>();
        while (query.MoveNext(out var uid, out var pvs))
        {
            if (_timing.CurTime <= pvs.NextUpdate)
                continue;

            pvs.NextUpdate = _timing.CurTime + UpdateFrequency;

            if (InPvs(uid))
            {
                pvs.DespawnAttempt = pvs.MaxDespawnAttempt;
            }
            else
            {
                pvs.DespawnAttempt--;
            }

            if (pvs.DespawnAttempt <= 0)
            {
                _adminLogger.Add(LogType.EntityDelete, LogImpact.Medium, $"{ToPrettyString(uid):entity} was out of the players' pvs for too long and was deleted");
                QueueDel(uid);
            }
        }
    }

    public bool InPvs(EntityUid uid)
    {
        var nearMinds = _lookup.GetEntitiesInRange<MindContainerComponent>(Transform(uid).Coordinates, CVars.NetMaxUpdateRange.DefaultValue / 2);
        foreach (var mind in nearMinds)
        {
            if (HasComp<GhostComponent>(mind))
                continue;

            if (!mind.Comp.HasMind)
                continue;

            return true;
        }

        return false;
    }
}
