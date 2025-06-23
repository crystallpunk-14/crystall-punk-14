using Content.Shared._CP14.MagicVision;
using Robust.Client.GameObjects;
using Robust.Client.Timing;
using Robust.Shared.Console;

namespace Content.Client._CP14.MagicVision;

public sealed class CP14ClientMagicVisionSystem : CP14SharedMagicVisionSystem
{
    [Dependency] private readonly IClientGameTiming _timing = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;


    private bool _markersVisible;

    public bool MarkersVisible
    {
        get => _markersVisible;
        set
        {
            _markersVisible = value;
            UpdateVisibilityAll();
        }
    }

    private TimeSpan _nextUpdate = TimeSpan.Zero;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicVisionMarkerComponent, ComponentStartup>(OnStartupMarker);
        SubscribeLocalEvent<CP14MagicVisionMarkerComponent, AfterAutoHandleStateEvent>(OnHandleStateMarker);
    }

    private void OnHandleStateMarker(Entity<CP14MagicVisionMarkerComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;
        if (ent.Comp.Icon is null)
            return;

        var layer = _sprite.AddLayer(ent.Owner, ent.Comp.Icon);
        sprite.LayerSetShader(layer, "unshaded");
    }

    private void OnStartupMarker(Entity<CP14MagicVisionMarkerComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        UpdateVisibility(ent, sprite);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_timing.CurTime < _nextUpdate)
            return;

        _nextUpdate = _timing.CurTime + TimeSpan.FromSeconds(0.5f);
        var queryFade = EntityQueryEnumerator<CP14MagicVisionMarkerComponent, SpriteComponent>();
        while (queryFade.MoveNext(out var uid, out var fade, out var sprite))
        {
            UpdateOpaque((uid, fade), sprite);
        }
    }

    private void UpdateVisibility(Entity<CP14MagicVisionMarkerComponent> ent, SpriteComponent sprite)
    {
        _sprite.SetVisible(ent.Owner, _markersVisible);
    }

    private void UpdateOpaque(Entity<CP14MagicVisionMarkerComponent> ent, SpriteComponent sprite)
    {
        var progress = Math.Clamp((_timing.CurTime.TotalSeconds - ent.Comp.SpawnTime.TotalSeconds) / (ent.Comp.EndTime.TotalSeconds - ent.Comp.SpawnTime.TotalSeconds), 0, 1);
        var alpha = 1 - progress;
        _sprite.SetColor(ent.Owner, Color.White.WithAlpha((float)alpha));
    }

    private void UpdateVisibilityAll()
    {
        var query = AllEntityQuery<CP14MagicVisionMarkerComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var marker, out var sprite))
        {
            UpdateVisibility((uid, marker), sprite);
        }
    }
}

internal sealed class ShowMagicCommand : LocalizedCommands
{
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

    public override string Command => "cp14_showmagic";

    public override string Help => "Toggle visibility of magic traces";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        _entitySystemManager.GetEntitySystem<CP14ClientMagicVisionSystem>().MarkersVisible ^= true;
    }
}
