using Content.Server._CP14.Demiplane;
using Content.Server._CP14.RoundStatistic.DemiplaneDeath;
using Content.Shared._CP14.Demiplane.Components;
using Content.Shared.GameTicking;

namespace Content.Server._CP14.RoundStatistic;

public sealed partial class CP14RoundStatTrackerSystem
{
    private void InitializeDemiplaneDeath()
    {
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnSpawnComplete);
        SubscribeLocalEvent<CP14DeathDemiplaneStatisticComponent, EntityTerminatingEvent>(OnEntityTerminated);
        SubscribeLocalEvent<CP14DeathDemiplaneStatisticComponent, CP14DemiplaneUnsafeExit>(OnDemiplaneUnsafeExit);
    }

    private void OnSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        EnsureComp<CP14DeathDemiplaneStatisticComponent>(ev.Mob);
    }

    private void OnDemiplaneUnsafeExit(Entity<CP14DeathDemiplaneStatisticComponent> ent, ref CP14DemiplaneUnsafeExit args)
    {
        TrackAdd(ent.Comp.Statistic, 1);
    }

    //For round remove variants, like gibs or chasm falls
    private void OnEntityTerminated(Entity<CP14DeathDemiplaneStatisticComponent> ent, ref EntityTerminatingEvent args)
    {
        if (!HasComp<CP14DemiplaneComponent>(Transform(ent).MapUid))
            return;

        TrackAdd(ent.Comp.Statistic, 1);
    }
}
