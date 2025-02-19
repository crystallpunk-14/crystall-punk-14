using Content.Shared._CP14.Roof;
using Content.Shared.Ghost;
using Content.Shared.Light.EntitySystems;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Console;
using Robust.Shared.Map.Components;

namespace Content.Client._CP14.Roof;

public sealed class CP14RoofSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedRoofSystem _roof = default!;

    private bool _roofVisible = true;
    public bool DisabledByCommand = false;

    private EntityQuery<MapGridComponent> _gridQuery;
    private EntityQuery<GhostComponent> _ghostQuery;
    private EntityQuery<TransformComponent> _xformQuery;

    public bool RoofVisibility
    {
        get => _roofVisible && !DisabledByCommand;
        set
        {
            _roofVisible = value;
            UpdateRoofVisibilityAll();
        }
    }

    public override void Initialize()
    {
        base.Initialize();

        _ghostQuery = GetEntityQuery<GhostComponent>();
        _xformQuery = GetEntityQuery<TransformComponent>();
        _gridQuery = GetEntityQuery<MapGridComponent>();

        SubscribeLocalEvent<CP14RoofComponent, ComponentStartup>(RoofStartup);
        SubscribeLocalEvent<CP14RoofComponent, ComponentRemove>(RoofRemove);

        SubscribeLocalEvent<GhostComponent, CP14ToggleRoofVisibilityAction>(OnToggleRoof);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var player = _playerManager.LocalEntity;

        if (_ghostQuery.HasComp(player))
            return;

        if (_xformQuery.TryComp(player, out var playerXform))
        {
            var grid = playerXform.GridUid;
            if (grid == null || !TryComp<MapGridComponent>(grid, out var gridComp))
                return;

            var roofQuery = GetEntityQuery<CP14RoofComponent>();
            var anchored = _map.GetAnchoredEntities(grid.Value, gridComp, playerXform.Coordinates);

            var underRoof = false;
            foreach (var ent in anchored)
            {
                if (!roofQuery.HasComp(ent))
                    continue;

                underRoof = true;
            }
            if (underRoof && _roofVisible)
            {
                RoofVisibility = false;
            }
            if (!underRoof && !_roofVisible)
            {
                RoofVisibility = true;
            }
        }
    }

    private void OnToggleRoof(Entity<GhostComponent> ent, ref CP14ToggleRoofVisibilityAction args)
    {
        if (args.Handled)
            return;

        DisabledByCommand = !DisabledByCommand;
        UpdateRoofVisibilityAll();

        args.Handled = true;
    }

    private void RoofStartup(Entity<CP14RoofComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        UpdateVisibility(ent, sprite);

        //var xform = Transform(ent);
//
        //if (_gridQuery.TryComp(xform.GridUid, out var grid))
        //{
        //    var index = _map.LocalToTile(xform.GridUid.Value, grid, xform.Coordinates);
        //    _roof.SetRoof((xform.GridUid.Value, grid, null), index, true);
        //}
    }

    private void RoofRemove(Entity<CP14RoofComponent> ent, ref ComponentRemove args)
    {
        //var xform = Transform(ent);
//
        //if (_gridQuery.TryComp(xform.GridUid, out var grid))
        //{
        //    var index = _map.LocalToTile(xform.GridUid.Value, grid, xform.Coordinates);
        //    _roof.SetRoof((xform.GridUid.Value, grid, null), index, false);
        //}
    }

    private void UpdateVisibility(Entity<CP14RoofComponent> ent, SpriteComponent sprite)
    {
        sprite.Visible = RoofVisibility;
    }

    public void UpdateRoofVisibilityAll()
    {
        var query = EntityQueryEnumerator<CP14RoofComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var marker, out var sprite))
        {
            UpdateVisibility((uid, marker), sprite);
        }
    }
}

internal sealed class ShowRoof : LocalizedCommands
{
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

    public override string Command => "cp14_toggleroof";

    public override string Help => "Toggle roof visibility";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var roofSystem = _entitySystemManager.GetEntitySystem<CP14RoofSystem>();
        roofSystem.DisabledByCommand = !roofSystem.DisabledByCommand;
        roofSystem.UpdateRoofVisibilityAll();
    }
}
