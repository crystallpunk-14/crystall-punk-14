using Content.Client.Gameplay;
using Content.Shared._CP14.Roof;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Client.State;
using Robust.Shared.Console;
using Robust.Shared.Map.Components;

namespace Content.Client._CP14.Roof;

public sealed class CP14RoofSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IStateManager _stateManager = default!;
    [Dependency] private readonly OccluderSystem _occluder = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;

    private bool _roofVisible = true;

    public bool RoofVisibility
    {
        get => _roofVisible;
        set
        {
            _roofVisible = value;
            UpdateRoofVisibilityAll();
        }
    }

    public override void Initialize()
    {
        base.Initialize();


        SubscribeLocalEvent<CP14RoofComponent, ComponentStartup>(RoofStartup);
        SubscribeLocalEvent<EyeComponent, CP14ToggleRoofVisibilityEvent>(OnToggleRoof);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var player = _playerManager.LocalEntity;
        var spriteQuery = GetEntityQuery<SpriteComponent>();

        if (TryComp(player, out TransformComponent? playerXform))
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

    private void OnToggleRoof(Entity<EyeComponent> ent, ref CP14ToggleRoofVisibilityEvent args)
    {
        RoofVisibility = !_roofVisible;
    }

    private void RoofStartup(Entity<CP14RoofComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        UpdateVisibility(ent, sprite);
    }

    private void UpdateVisibility(Entity<CP14RoofComponent> ent, SpriteComponent sprite)
    {
        sprite.Visible = _roofVisible;
    }

    private void UpdateRoofVisibilityAll()
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
        _entitySystemManager.GetEntitySystem<CP14RoofSystem>().RoofVisibility ^= true;
    }
}
