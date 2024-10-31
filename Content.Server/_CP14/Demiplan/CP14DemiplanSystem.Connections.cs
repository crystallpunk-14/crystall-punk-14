using Content.Shared._CP14.Demiplan.Components;

namespace Content.Server._CP14.Demiplan;

public sealed partial class CP14DemiplanSystem
{
    private void InitConnections()
    {
        SubscribeLocalEvent<CP14DemiplanExitPointComponent, ComponentShutdown>(ExitPointShutdown);

        SubscribeLocalEvent<CP14DemiplanEntryPointComponent, ComponentShutdown>(EntryPointShutdown);
        SubscribeLocalEvent<CP14DemiplanEntryPointComponent, MapInitEvent>(EntryPointMapInit);
    }

    private void ExitPointShutdown(Entity<CP14DemiplanExitPointComponent> exitPoint, ref ComponentShutdown args)
    {
        if (exitPoint.Comp.Link is null)
            return;

        RemoveDemiplanConnection(exitPoint.Comp.Link.Value, exitPoint);
    }

    private void EntryPointShutdown(Entity<CP14DemiplanEntryPointComponent> entryPoint, ref ComponentShutdown args)
    {
        if (entryPoint.Comp.Link is null)
            return;

        RemoveDemiplanEntryPoint(entryPoint.Comp.Link.Value, entryPoint);
    }


    private void EntryPointMapInit(Entity<CP14DemiplanEntryPointComponent> entryPoint, ref MapInitEvent args)
    {
        var map = Transform(entryPoint).MapUid;
        if (!TryComp<CP14DemiplanComponent>(map, out var demiplan))
        {
            QueueDel(entryPoint);
            return;
        }

        AddDemiplanEntryPoint((map.Value, demiplan), entryPoint);
    }

    private void AddDemiplanConnection(Entity<CP14DemiplanComponent> demiplan,
        Entity<CP14DemiplanExitPointComponent> exitPoint)
    {
        if (demiplan.Comp.ExitPoints.Contains(exitPoint))
            return;

        demiplan.Comp.ExitPoints.Add(exitPoint);
        exitPoint.Comp.Link = demiplan;
    }

    private void RemoveDemiplanConnection(Entity<CP14DemiplanComponent> demiplan,
        Entity<CP14DemiplanExitPointComponent> exitPoint)
    {
        if (!demiplan.Comp.ExitPoints.Contains(exitPoint))
            return;

        demiplan.Comp.ExitPoints.Remove(exitPoint);
        exitPoint.Comp.Link = null;
    }

    private void AddDemiplanEntryPoint(Entity<CP14DemiplanComponent> demiplan,
        Entity<CP14DemiplanEntryPointComponent> entryPoint)
    {
        if (demiplan.Comp.EntryPoints.Contains(entryPoint))
            return;

        demiplan.Comp.EntryPoints.Add(entryPoint);
        entryPoint.Comp.Link = demiplan;
    }

    private void RemoveDemiplanEntryPoint(Entity<CP14DemiplanComponent> demiplan,
        Entity<CP14DemiplanEntryPointComponent> entryPoint)
    {
        if (!demiplan.Comp.EntryPoints.Contains(entryPoint))
            return;

        demiplan.Comp.EntryPoints.Remove(entryPoint);
        entryPoint.Comp.Link = null;
    }
}
