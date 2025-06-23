using Content.Shared._CP14.Roof;
using Content.Shared.Ghost;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Console;
using Robust.Shared.Map.Components;

namespace Content.Client._CP14.Roof;

public sealed class CP14RoofSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public bool DisabledByCommand;

    private EntityQuery<GhostComponent> _ghostQuery;
    private EntityQuery<TransformComponent> _xformQuery;

    private bool _roofVisible = true;
    public bool RoofVisible
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

        SubscribeLocalEvent<CP14RoofComponent, ComponentStartup>(RoofStartup);

        SubscribeLocalEvent<GhostComponent, CP14ToggleRoofVisibilityAction>(OnToggleRoof);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var player = _playerManager.LocalEntity;

        if (_ghostQuery.HasComp(player))
            return;

        if (!_xformQuery.TryComp(player, out var playerXform))
            return;

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
            RoofVisible = false;
        }
        if (!underRoof && !_roofVisible)
        {
            RoofVisible = true;
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
    }

    private void UpdateVisibility(Entity<CP14RoofComponent> ent, SpriteComponent sprite)
    {
        _sprite.SetVisible((ent, sprite), RoofVisible);
    }

    public void UpdateRoofVisibilityAll()
    {
        var query = AllEntityQuery<CP14RoofComponent, SpriteComponent>();
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
