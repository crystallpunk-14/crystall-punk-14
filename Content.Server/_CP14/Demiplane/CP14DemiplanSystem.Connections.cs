using Content.Shared._CP14.Demiplane.Components;
using Robust.Shared.Random;

namespace Content.Server._CP14.Demiplane;

public sealed partial class CP14DemiplaneSystem
{
    private void InitConnections()
    {
        SubscribeLocalEvent<CP14DemiplaneRiftComponent, MapInitEvent>(OnRiftInit);
        SubscribeLocalEvent<CP14DemiplaneRiftComponent, ComponentShutdown>(OnRiftShutdown);
    }

    private void OnRiftInit(Entity<CP14DemiplaneRiftComponent> rift, ref MapInitEvent args)
    {
        var map = Transform(rift).MapUid;
        if (TryComp<CP14DemiplaneComponent>(map, out var demiplan)) // In demiplan
        {
            if (rift.Comp.TryAutoLinkToMap)
                rift.Comp.Demiplan = (map.Value, demiplan);

            if (rift.Comp.ActiveTeleport)
                AddDemiplanRandomEntryPoint((map.Value, demiplan), rift);
        }
        else if (rift.Comp.Demiplan is not null) //We out of demiplan
        {
            if (rift.Comp.ActiveTeleport)
                AddDemiplanRandomExitPoint(rift.Comp.Demiplan.Value, rift);
        }
    }

    private void OnRiftShutdown(Entity<CP14DemiplaneRiftComponent> rift, ref ComponentShutdown args)
    {
        if (rift.Comp.Demiplan is null)
            return;

        RemoveDemiplanRandomEntryPoint(rift.Comp.Demiplan, rift);
        RemoveDemiplanRandomExitPoint(rift.Comp.Demiplan, rift);
    }

    /// <summary>
    ///Add a position in the real world where you can get out of this demiplan
    /// </summary>
    private void AddDemiplanRandomExitPoint(Entity<CP14DemiplaneComponent> demiplan,
        Entity<CP14DemiplaneRiftComponent> exitPoint)
    {
        if (demiplan.Comp.ExitPoints.Contains(exitPoint))
            return;

        demiplan.Comp.ExitPoints.Add(exitPoint);
        exitPoint.Comp.Demiplan = demiplan;
    }

    /// <summary>
    /// Removing the demiplan exit point, one of which the player can exit to
    /// </summary>
    private void RemoveDemiplanRandomExitPoint(Entity<CP14DemiplaneComponent>? demiplan,
        Entity<CP14DemiplaneRiftComponent> exitPoint)
    {
        if (demiplan is not null && demiplan.Value.Comp.ExitPoints.Contains(exitPoint))
        {
            demiplan.Value.Comp.ExitPoints.Remove(exitPoint);
            exitPoint.Comp.Demiplan = null;
        }

        if (exitPoint.Comp.DeleteAfterDisconnect)
            QueueDel(exitPoint);
    }

    /// <summary>
    /// Add a position within the demiplan that can be entered into the demiplan
    /// </summary>
    private void AddDemiplanRandomEntryPoint(Entity<CP14DemiplaneComponent> demiplan,
        Entity<CP14DemiplaneRiftComponent> entryPoint)
    {
        if (demiplan.Comp.EntryPoints.Contains(entryPoint))
            return;

        demiplan.Comp.EntryPoints.Add(entryPoint);
        entryPoint.Comp.Demiplan = demiplan;
    }

    private void RemoveDemiplanRandomEntryPoint(Entity<CP14DemiplaneComponent>? demiplan,
        Entity<CP14DemiplaneRiftComponent> entryPoint)
    {
        if (demiplan is not null && demiplan.Value.Comp.EntryPoints.Contains(entryPoint))
        {
            demiplan.Value.Comp.EntryPoints.Remove(entryPoint);
            entryPoint.Comp.Demiplan = null;
        }

        if (entryPoint.Comp.DeleteAfterDisconnect)
            QueueDel(entryPoint);
    }

    public bool TryGetDemiplanEntryPoint(Entity<CP14DemiplaneComponent> demiplan, out Entity<CP14DemiplaneRiftComponent>? entryPoint)
    {
        entryPoint = null;

        if (demiplan.Comp.EntryPoints.Count == 0)
            return false;

        entryPoint = _random.Pick(demiplan.Comp.EntryPoints);
        return true;
    }

    public bool TryGetDemiplanExitPoint(Entity<CP14DemiplaneComponent> demiplan,
        out Entity<CP14DemiplaneRiftComponent>? exitPoint)
    {
        exitPoint = null;

        if (demiplan.Comp.ExitPoints.Count == 0)
            return false;

        exitPoint = _random.Pick(demiplan.Comp.ExitPoints);
        return true;
    }
}
