using Content.Shared._CP14.Demiplan.Components;
using Robust.Shared.Random;

namespace Content.Server._CP14.Demiplan;

public sealed partial class CP14DemiplanSystem
{
    private void InitConnections()
    {
        SubscribeLocalEvent<CP14DemiplanRiftComponent, MapInitEvent>(OnRiftInit);
        SubscribeLocalEvent<CP14DemiplanRiftComponent, ComponentShutdown>(OnRiftShutdown);
    }

    private void OnRiftInit(Entity<CP14DemiplanRiftComponent> rift, ref MapInitEvent args)
    {
        var map = Transform(rift).MapUid;
        if (TryComp<CP14DemiplanComponent>(map, out var demiplan)) // In demiplan
        {
            if (rift.Comp.TryAutoLinkToMap)
                rift.Comp.Demiplan = (map.Value, demiplan);

            if (rift.Comp.ActiveTeleportPoint)
                AddDemiplanRandomEntryPoint((map.Value, demiplan), rift);
        }
        else if (rift.Comp.Demiplan is not null) //We out of demiplan
        {
            if (rift.Comp.ActiveTeleportPoint)
                AddDemiplanRandomExitPoint(rift.Comp.Demiplan.Value, rift);
        }
    }

    private void OnRiftShutdown(Entity<CP14DemiplanRiftComponent> rift, ref ComponentShutdown args)
    {
        if (rift.Comp.Demiplan is null)
            return;

        RemoveDemiplanRandomEntryPoint(rift.Comp.Demiplan.Value, rift);
        RemoveDemiplanRandomExitPoint(rift.Comp.Demiplan.Value, rift);
    }

    /// <summary>
    ///Add a position in the real world where you can get out of this demiplan
    /// </summary>
    private void AddDemiplanRandomExitPoint(Entity<CP14DemiplanComponent> demiplan,
        Entity<CP14DemiplanRiftComponent> exitPoint)
    {
        if (demiplan.Comp.ExitPoints.Contains(exitPoint))
            return;

        demiplan.Comp.ExitPoints.Add(exitPoint);
        exitPoint.Comp.Demiplan = demiplan;
    }

    private void RemoveDemiplanRandomExitPoint(Entity<CP14DemiplanComponent> demiplan,
        Entity<CP14DemiplanRiftComponent> exitPoint)
    {
        if (!demiplan.Comp.ExitPoints.Contains(exitPoint))
            return;

        demiplan.Comp.ExitPoints.Remove(exitPoint);
        exitPoint.Comp.Demiplan = null;
    }

    /// <summary>
    /// Add a position within the demiplan that can be entered into the demiplan
    /// </summary>
    private void AddDemiplanRandomEntryPoint(Entity<CP14DemiplanComponent> demiplan,
        Entity<CP14DemiplanRiftComponent> entryPoint)
    {
        if (demiplan.Comp.EntryPoints.Contains(entryPoint))
            return;

        demiplan.Comp.EntryPoints.Add(entryPoint);
        entryPoint.Comp.Demiplan = demiplan;
    }

    private void RemoveDemiplanRandomEntryPoint(Entity<CP14DemiplanComponent> demiplan,
        Entity<CP14DemiplanRiftComponent> entryPoint)
    {
        if (!demiplan.Comp.EntryPoints.Contains(entryPoint))
            return;

        demiplan.Comp.EntryPoints.Remove(entryPoint);
        entryPoint.Comp.Demiplan = null;
    }

    public bool TryGetDemiplanEntryPoint(Entity<CP14DemiplanComponent> demiplan, out Entity<CP14DemiplanRiftComponent>? entryPoint)
    {
        entryPoint = null;

        if (demiplan.Comp.EntryPoints.Count == 0)
            return false;

        entryPoint = _random.Pick(demiplan.Comp.EntryPoints);
        return true;
    }

    public bool TryGetDemiplanExitPoint(Entity<CP14DemiplanComponent> demiplan,
        out Entity<CP14DemiplanRiftComponent>? exitPoint)
    {
        exitPoint = null;

        if (demiplan.Comp.ExitPoints.Count == 0)
            return false;

        exitPoint = _random.Pick(demiplan.Comp.ExitPoints);
        return true;
    }
}
