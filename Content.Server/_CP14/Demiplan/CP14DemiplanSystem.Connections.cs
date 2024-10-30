using Content.Shared._CP14.Demiplan.Components;

namespace Content.Server._CP14.Demiplan;

public sealed partial class CP14DemiplanSystem
{
    private void InitConnections()
    {
        SubscribeLocalEvent<CP14DemiplanConnectionComponent, ComponentShutdown>(ConnectionShutdown);

        SubscribeLocalEvent<CP14DemiplanEntryPointComponent, ComponentShutdown>(EntryPointShutdown);
        SubscribeLocalEvent<CP14DemiplanEntryPointComponent, MapInitEvent>(EntryPointMapInit);
    }

    private void ConnectionShutdown(Entity<CP14DemiplanConnectionComponent> connection, ref ComponentShutdown args)
    {
        if (connection.Comp.Link is null)
            return;

        RemoveDemiplanConnection(connection.Comp.Link.Value, connection);
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
        Entity<CP14DemiplanConnectionComponent> connection)
    {
        if (demiplan.Comp.Connections.Contains(connection))
            return;

        demiplan.Comp.Connections.Add(connection);
        connection.Comp.Link = demiplan;
    }

    private void RemoveDemiplanConnection(Entity<CP14DemiplanComponent> demiplan,
        Entity<CP14DemiplanConnectionComponent> connection)
    {
        if (!demiplan.Comp.Connections.Contains(connection))
            return;

        demiplan.Comp.Connections.Remove(connection);
        connection.Comp.Link = null;
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
