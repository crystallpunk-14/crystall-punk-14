using Content.Shared.Placeable;

namespace Content.Server._CP14.Workbench;

public sealed partial class CP14WorkbenchSystem
{
    private void InitProviders()
    {
        SubscribeLocalEvent<CP14WorkbenchPlaceableProviderComponent, CP14WorkbenchGetResourcesEvent>(OnGetResource);
    }

    private void OnGetResource(Entity<CP14WorkbenchPlaceableProviderComponent> ent, ref CP14WorkbenchGetResourcesEvent args)
    {
        if (!TryComp<ItemPlacerComponent>(ent, out var placer))
            return;

        args.AddResources(placer.PlacedEntities);
    }
}

public sealed class CP14WorkbenchGetResourcesEvent : EntityEventArgs
{
    public HashSet<EntityUid> Resources { get; private set; } = new();

    public void AddResource(EntityUid resource)
    {
        Resources.Add(resource);
    }

    public void AddResources(IEnumerable<EntityUid> resources)
    {
        foreach (var resource in resources)
        {
            Resources.Add(resource);
        }
    }
}
