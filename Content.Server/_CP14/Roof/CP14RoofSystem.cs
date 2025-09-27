using Content.Shared.Light.EntitySystems;
using Robust.Shared.Map.Components;

namespace Content.Server._CP14.Roof;

/// <inheritdoc/>
public sealed class CP14RoofSystem : EntitySystem
{
    [Dependency] private readonly SharedMapSystem _maps = default!;
    [Dependency] private readonly SharedRoofSystem _roof = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14SetGridRoovedComponent, ComponentStartup>(OnRoofStartup);
        SubscribeLocalEvent<CP14SetGridUnroovedComponent, ComponentStartup>(OnRoofStartup);
        SubscribeLocalEvent<CP14SetGridRoovedComponent, TileChangedEvent>(OnTileChanged);
    }

    private void OnTileChanged(Entity<CP14SetGridRoovedComponent> ent, ref TileChangedEvent args)
    {
        foreach (var changed in args.Changes)
        {
            if (changed.OldTile.IsEmpty)
                _roof.SetRoof(ent.Owner, changed.GridIndices, true);
        }
    }

    private void OnRoofStartup(Entity<CP14SetGridRoovedComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<MapGridComponent>(ent.Owner, out var gridComp))
            return;

        var enumerator = _maps.GetAllTilesEnumerator(ent, gridComp);
        while (enumerator.MoveNext(out var tileRef))
        {
            _roof.SetRoof(ent.Owner, tileRef.Value.GridIndices, true);
        }
    }

    private void OnRoofStartup(Entity<CP14SetGridUnroovedComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<MapGridComponent>(ent.Owner, out var gridComp))
            return;

        var enumerator = _maps.GetAllTilesEnumerator(ent, gridComp);
        while (enumerator.MoveNext(out var tileRef))
        {
            _roof.SetRoof(ent.Owner, tileRef.Value.GridIndices, false);
        }
    }
}
