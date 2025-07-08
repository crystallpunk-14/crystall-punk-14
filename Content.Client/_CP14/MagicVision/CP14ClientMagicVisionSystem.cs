using System.Numerics;
using Content.Shared._CP14.MagicVision;
using Content.Shared.Examine;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.Timing;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Client._CP14.MagicVision;

public sealed class CP14ClientMagicVisionSystem : CP14SharedMagicVisionSystem
{
    [Dependency] private readonly IClientGameTiming _timing = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private CP14MagicVisionOverlay? _overlay;
    private CP14MagicVisionNoirOverlay? _overlay2;

    private TimeSpan _nextUpdate = TimeSpan.Zero;

    private SoundSpecifier _startSound = new SoundPathSpecifier(new ResPath("/Audio/Effects/eye_open.ogg"));
    private SoundSpecifier _endSound = new SoundPathSpecifier(new ResPath("/Audio/Effects/eye_close.ogg"));

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicVisionMarkerComponent, AfterAutoHandleStateEvent>(OnHandleStateMarker);

        SubscribeLocalEvent<CP14MagicVisionComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<CP14MagicVisionComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        SubscribeLocalEvent<CP14MagicVisionComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<CP14MagicVisionComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnComponentShutdown(Entity<CP14MagicVisionComponent> ent, ref ComponentShutdown args)
    {
        if (_player.LocalEntity != ent)
            return;
        if (_overlay != null)
        {
            _overlayMan.RemoveOverlay(_overlay);
            _overlay = null;
        }
        if (_overlay2 != null)
        {
            _overlayMan.RemoveOverlay(_overlay2);
            _overlay2 = null;
        }

        _audio.PlayGlobal(_endSound, ent);
    }

    private void OnComponentInit(Entity<CP14MagicVisionComponent> ent, ref ComponentInit args)
    {
        if (_player.LocalEntity != ent)
            return;

        _overlay = new CP14MagicVisionOverlay();
        _overlayMan.AddOverlay(_overlay);
        _overlay.StartOverlay = _timing.CurTime;

        _overlay2 = new CP14MagicVisionNoirOverlay();
        _overlayMan.AddOverlay(_overlay2);

        _audio.PlayGlobal(_startSound, ent);
    }

    private void OnPlayerAttached(Entity<CP14MagicVisionComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        _overlay = new CP14MagicVisionOverlay();
        _overlayMan.AddOverlay(_overlay);
        _overlay.StartOverlay = _timing.CurTime;

        _overlay2 = new CP14MagicVisionNoirOverlay();
        _overlayMan.AddOverlay(_overlay2);

        _audio.PlayGlobal(_startSound, ent);
    }

    private void OnPlayerDetached(Entity<CP14MagicVisionComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        if (_overlay != null)
        {
            _overlayMan.RemoveOverlay(_overlay);
            _overlay = null;
        }
        if (_overlay2 != null)
        {
            _overlayMan.RemoveOverlay(_overlay2);
            _overlay2 = null;
        }
        _audio.PlayGlobal(_endSound, ent);
    }

    protected override void OnExamined(Entity<CP14MagicVisionMarkerComponent> ent, ref ExaminedEvent args)
    {
        base.OnExamined(ent, ref args);

        if (ent.Comp.TargetCoordinates is null)
            return;

        var originPosition = _transform.GetWorldPosition(ent);
        var targetPosition = _transform.ToWorldPosition(ent.Comp.TargetCoordinates.Value);

        if ((targetPosition - originPosition).Length() < 0.5f)
            return;

        Angle angle = new(targetPosition - originPosition);

        var pointer = Spawn(ent.Comp.PointerProto, new MapCoordinates(originPosition, _transform.GetMapId(Transform(ent).Coordinates)));

        _transform.SetWorldRotation(pointer, angle + Angle.FromDegrees(90));
    }

    private void OnHandleStateMarker(Entity<CP14MagicVisionMarkerComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;
        if (ent.Comp.Icon is null)
            return;

        var layer = _sprite.AddLayer(ent.Owner, ent.Comp.Icon);
        sprite.LayerSetShader(layer, "unshaded");
        _sprite.LayerSetScale(ent.Owner, layer, new Vector2(0.5f, 0.5f));
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

    private void UpdateOpaque(Entity<CP14MagicVisionMarkerComponent> ent, SpriteComponent sprite)
    {
        var progress = Math.Clamp((_timing.CurTime.TotalSeconds - ent.Comp.SpawnTime.TotalSeconds) / (ent.Comp.EndTime.TotalSeconds - ent.Comp.SpawnTime.TotalSeconds), 0, 1);
        var alpha = 1 - progress;
        _sprite.SetColor(ent.Owner, Color.White.WithAlpha((float)alpha));
    }
}
