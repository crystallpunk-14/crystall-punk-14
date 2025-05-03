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
        if (_demiplaneQuery.TryComp(map, out var demiplane)) // In demiplane
        {
            if (rift.Comp.TryAutoLinkToMap)
                rift.Comp.Demiplane = map.Value;

            if (rift.Comp.ActiveTeleport)
                AddDemiplaneRandomEntryPoint((map.Value, demiplane), rift);
        }
        else if (rift.Comp.Demiplane is not null) //We out of demiplane
        {
            if (_demiplaneQuery.TryComp(rift.Comp.Demiplane, out var riftDemiplane))
            {
                if (rift.Comp.ActiveTeleport)
                    AddDemiplaneRandomExitPoint((rift.Comp.Demiplane.Value, riftDemiplane), rift);
            }
        }
    }

    private void OnRiftShutdown(Entity<CP14DemiplaneRiftComponent> rift, ref ComponentShutdown args)
    {
        if (rift.Comp.Demiplane is null)
            return;

        if (!_demiplaneQuery.TryComp(rift.Comp.Demiplane, out var riftDemiplane))
            return;

        RemoveDemiplaneRandomEntryPoint((rift.Comp.Demiplane.Value, riftDemiplane), rift);
        RemoveDemiplaneRandomExitPoint((rift.Comp.Demiplane.Value, riftDemiplane), rift);
    }

    /// <summary>
    ///Add a position in the real world where you can get out of this demiplane
    /// </summary>
    private void AddDemiplaneRandomExitPoint(Entity<CP14DemiplaneComponent> demiplane,
        Entity<CP14DemiplaneRiftComponent> exitPoint)
    {
        demiplane.Comp.ExitPoints.Add(exitPoint);
        exitPoint.Comp.Demiplane = demiplane;
    }

    /// <summary>
    /// Removing the demiplane exit point, one of which the player can exit to
    /// </summary>
    private void RemoveDemiplaneRandomExitPoint(Entity<CP14DemiplaneComponent>? demiplane,
        EntityUid exitPoint)
    {
        if (!TryComp<CP14DemiplaneRiftComponent>(exitPoint, out var riftComp))
            return;

        if (demiplane is not null && demiplane.Value.Comp.ExitPoints.Contains(exitPoint))
        {
            demiplane.Value.Comp.ExitPoints.Remove(exitPoint);
            riftComp.Demiplane = null;
        }

        if (riftComp.DeleteAfterDisconnect && exitPoint.Valid)
            QueueDel(exitPoint);
    }

    /// <summary>
    /// Add a position within the demiplane that can be entered into the demiplane
    /// </summary>
    private void AddDemiplaneRandomEntryPoint(Entity<CP14DemiplaneComponent> demiplane,
        Entity<CP14DemiplaneRiftComponent> entryPoint)
    {
        demiplane.Comp.EntryPoints.Add(entryPoint);
        entryPoint.Comp.Demiplane = demiplane;
    }

    private void RemoveDemiplaneRandomEntryPoint(Entity<CP14DemiplaneComponent>? demiplane,
        EntityUid entryPoint)
    {
        if (!TryComp<CP14DemiplaneRiftComponent>(entryPoint, out var riftComp))
            return;

        if (demiplane is not null && demiplane.Value.Comp.EntryPoints.Contains(entryPoint))
        {
            demiplane.Value.Comp.EntryPoints.Remove(entryPoint);
            riftComp.Demiplane = null;
        }

        if (riftComp.DeleteAfterDisconnect && entryPoint.Valid)
            QueueDel(entryPoint);
    }

    public bool TryGetDemiplaneEntryPoint(Entity<CP14DemiplaneComponent> demiplane, out EntityUid? entryPoint)
    {
        entryPoint = null;

        if (demiplane.Comp.EntryPoints.Count == 0)
            return false;

        entryPoint = _random.Pick(demiplane.Comp.EntryPoints);
        return true;
    }

    public bool TryGetDemiplaneExitPoint(Entity<CP14DemiplaneComponent> demiplane,
        out EntityUid? exitPoint)
    {
        exitPoint = null;

        if (demiplane.Comp.ExitPoints.Count == 0)
            return false;

        exitPoint = _random.Pick(demiplane.Comp.ExitPoints);
        return true;
    }
}
