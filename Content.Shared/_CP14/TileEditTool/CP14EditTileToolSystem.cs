using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.TileEditTool;

public sealed class CP14EditTileToolSystem : EntitySystem
{
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CP14EditTileToolComponent, AfterInteractEvent>(OnTileClick);
        SubscribeLocalEvent<CP14EditTileToolComponent, CP14TileToolReplaceDoAfter>(OnDoAfterEnd);
    }

    private void OnTileClick(Entity<CP14EditTileToolComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach)
            return;

        var doAfterArgs =
            new DoAfterArgs(EntityManager,
                args.User,
                ent.Comp.Delay,
                new CP14TileToolReplaceDoAfter(GetNetCoordinates(args.ClickLocation)),
                ent)
            {
                BreakOnDamage = true,
                BlockDuplicate = false,
                BreakOnMove = true,
                BreakOnHandChange = true,
            };
        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnDoAfterEnd(Entity<CP14EditTileToolComponent> ent, ref CP14TileToolReplaceDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        var location = GetCoordinates(args.Coordinates);

        var map = _transform.GetMap(location);
        if (!TryComp<MapGridComponent>(map, out var gridComp))
            return;

        var tileRef = location.GetTileRef();

        if (tileRef is null)
            return;

        var tile = tileRef.Value.Tile.GetContentTileDefinition();

        if (!ent.Comp.TileReplace.TryGetValue(tile, out var replaceTile))
            return;

        args.Handled = true;
        
        _map.SetTile((map.Value, gridComp), location, new Tile(_proto.Index(replaceTile).TileId));
        _audio.PlayPredicted(ent.Comp.Sound, location, args.User);
    }
}
