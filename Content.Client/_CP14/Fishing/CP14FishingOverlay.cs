using System.Numerics;
using Content.Client.Resources;
using Content.Shared._CP14.Fishing.Components;
using Content.Shared._CP14.Fishing.Prototypes;
using Content.Shared._CP14.Fishing.Systems;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client._CP14.Fishing;

public sealed class CP14FishingOverlay : Overlay
{
    [Dependency] private readonly IComponentFactory _factory = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    private readonly SpriteSystem _sprite;
    private readonly TransformSystem _transform;
    private readonly CP14FishingProcessSystem _fishingProcess;

    private Texture _backgroundTexture = default!;
    private Texture _handleTopTexture = default!;
    private Texture _handleMiddleTexture = default!;
    private Texture _handleBottomTexture = default!;

    private Texture _lootTexture = default!;

    private Vector2 _backgroundOffset;
    private Vector2 _backgroundHandleOffset;
    private Vector2 _backgroundHandleSize;

    private Vector2 _progressOffset;
    private Vector2 _progressSize;

    private EntityUid _process = EntityUid.Invalid;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    public CP14FishingOverlay()
    {
        IoCManager.InjectDependencies(this);

        _sprite = _entity.System<SpriteSystem>();
        _transform = _entity.System<TransformSystem>();
        _fishingProcess = _entity.System<CP14FishingProcessSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (_player.LocalEntity is not { } localEntity)
            return;

        if (!_fishingProcess.TryGetByUser(localEntity, out var fishingProcess))
            return;

        // Refresh the texture cache, with a new fishing process
        if (_process != fishingProcess.Value.Owner)
        {
            _process = fishingProcess.Value.Owner;
            UpdateCachedStyleSheet(fishingProcess.Value.Comp.StyleSheet);

            var prototype = _prototype.Index(fishingProcess.Value.Comp.LootProtoId);
            var iconPath = CP14FishingIconComponent.DefaultTexturePath;

            if (prototype.Components.TryGetComponent(_factory.GetComponentName(typeof(CP14FishingIconComponent)), out var iconComponent))
            {
                var comp = (CP14FishingIconComponent) iconComponent;
                iconPath = comp.TexturePath;
            }

            _lootTexture = _resourceCache.GetTexture(iconPath);
        }

        // Getting the position of the player we will be working from
        var worldPosition = _transform.GetWorldPosition(localEntity);

        // Calculate the shift of the player relative to the bottom of the coordinates
        var playerOffset = fishingProcess.Value.Comp.PlayerPositionNormalized * _backgroundHandleSize;
        var playerHalfSize = fishingProcess.Value.Comp.PlayerHalfSizeNormalized * _backgroundHandleSize;

        var lootOffset = fishingProcess.Value.Comp.LootPositionNormalized * _backgroundHandleSize + Vector2.UnitX * _backgroundHandleSize.X / 2;

        DrawBackground(args.WorldHandle, worldPosition - _backgroundOffset);
        DrawProgress(args.WorldHandle, worldPosition - _backgroundOffset + _progressOffset, _progressSize, fishingProcess.Value.Comp.Progress);
        DrawHandle(args.WorldHandle, worldPosition - _backgroundOffset + _backgroundHandleOffset + playerOffset, playerHalfSize);
        DrawLoot(args.WorldHandle, worldPosition - _backgroundOffset + _backgroundHandleOffset + lootOffset);
    }

    private void DrawBackground(DrawingHandleWorld handle, Vector2 position)
    {
        handle.DrawTexture(_backgroundTexture, position);
    }

    private void DrawProgress(DrawingHandleWorld handle, Vector2 position, Vector2 size, float progress)
    {
        var vectorA = position;
        var vectorB = position + size with { Y = size.Y * progress };
        var box = new Box2(vectorA, vectorB);

        handle.DrawRect(box, Color.Aqua);
    }

    private void DrawHandle(DrawingHandleWorld handle, Vector2 position, Vector2 halfSize)
    {
        var cursor = position - halfSize;

        // Bottom
        handle.DrawTexture(_handleBottomTexture, cursor);
        cursor += new Vector2(0, _handleBottomTexture.Height / 32f);

        // Middle
        while (cursor.Y < position.Y + halfSize.Y - _handleTopTexture.Height / 32f)
        {
            handle.DrawTexture(_handleMiddleTexture, cursor);
            cursor += new Vector2(0, _handleMiddleTexture.Height / 32f);
        }

        // Front
        handle.DrawTexture(_handleTopTexture, cursor);
    }

    private void DrawLoot(DrawingHandleWorld handle, Vector2 position)
    {
        handle.DrawTexture(_lootTexture, position - _lootTexture.Size / 64f);
    }

    private void UpdateCachedStyleSheet(CP14FishingProcessStyleSheetPrototype styleSheet)
    {
        _backgroundTexture = _resourceCache.GetTexture(styleSheet.Background.Texture);
        _handleTopTexture = _resourceCache.GetTexture(styleSheet.Handle.TopTexture);
        _handleMiddleTexture = _resourceCache.GetTexture(styleSheet.Handle.MiddleTexture);
        _handleBottomTexture = _resourceCache.GetTexture(styleSheet.Handle.BottomTexture);

        _backgroundOffset = styleSheet.Background.Offset + Vector2.UnitX * _backgroundTexture.Width / 32f;

        _backgroundHandleOffset = styleSheet.Background.HandleOffset;
        _backgroundHandleSize = styleSheet.Background.HandleSize;

        _progressOffset = styleSheet.Background.ProgressOffset;
        _progressSize = styleSheet.Background.ProgressSize;
    }
}
