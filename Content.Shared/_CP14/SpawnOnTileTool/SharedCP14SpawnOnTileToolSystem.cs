using System.Linq;
using Content.Shared._CP14.Farming;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Popups;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Shared._CP14.SpawnOnTileTool;

public partial class SharedCP14SpawnOnTileToolSystem : EntitySystem
{
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDef = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CP14SpawnOnTileToolComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(Entity<CP14SpawnOnTileToolComponent> tool, ref AfterInteractEvent args)
    {
        var grid = _transform.GetGrid(args.ClickLocation);

        if (grid == null || !TryComp<MapGridComponent>(grid, out var gridComp))
            return;

        var tile = _map.GetTileRef(grid.Value, gridComp, args.ClickLocation);
        var tileDef = (ContentTileDefinition) _tileDef[tile.Tile.TypeId];

        if (tool.Comp.NeedEmptySpace && _map.GetAnchoredEntities(grid.Value, gridComp, args.ClickLocation).Count() > 0)
        {
            _popup.PopupClient(Loc.GetString("cp14-insufficient-space"), args.ClickLocation, args.User);
            return;
        }

        foreach (var pair in tool.Comp.Spawns)
        {
            if (tileDef.ID != pair.Key)
                continue;

            var doAfterArgs =
                new DoAfterArgs(EntityManager, args.User, tool.Comp.DoAfter, new SpawnOnTileToolAfterEvent(EntityManager, args.ClickLocation, pair.Value), tool)
                {
                    BreakOnDamage = true,
                    BlockDuplicate = true,
                    BreakOnMove = true,
                    BreakOnHandChange = true
                };
            _doAfter.TryStartDoAfter(doAfterArgs);
            break;
        }
    }
}
