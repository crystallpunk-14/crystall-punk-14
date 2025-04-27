using Content.Shared._Finster.FieldOfView;
using Content.Shared.Movement.Components;
using Content.Shared.Standing;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.Utility;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;

namespace Content.Client._Finster.FieldOfView;

public sealed class ClientFieldOfViewSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    //[Dependency] private readonly ZStackSystem _zStack = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;

    private EntityQuery<FieldOfViewComponent> _fovQuery;
    private EntityQuery<TransformComponent> _xformQuery;
    private EntityQuery<MobMoverComponent> _mobMoverQuery;
    private EntityQuery<SpriteComponent> _spriteQuery;
    private EntityQuery<MapComponent> _mapQuery;

    private List<EntityUid> _hiddenList = new();

    public override void Initialize()
    {
        base.Initialize();

        _fovQuery = GetEntityQuery<FieldOfViewComponent>();
        _xformQuery = GetEntityQuery<TransformComponent>();
        _mobMoverQuery = GetEntityQuery<MobMoverComponent>();
        _spriteQuery = GetEntityQuery<SpriteComponent>();
        _mapQuery = GetEntityQuery<MapComponent>();

        SubscribeLocalEvent<FieldOfViewComponent, LocalPlayerAttachedEvent>(OnAttached);
        SubscribeLocalEvent<FieldOfViewComponent, LocalPlayerDetachedEvent>(OnDetached);

        _overlay.AddOverlay(new FieldOfViewOverlay());
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay<FieldOfViewOverlay>();
    }

    private void OnAttached(Entity<FieldOfViewComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        RestoreHiddenList();
    }

    private void OnDetached(Entity<FieldOfViewComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        RestoreHiddenList();
    }

    private void RestoreHiddenList()
    {
        foreach (var uid in _hiddenList)
        {
            if (!_spriteQuery.TryComp(uid, out var spriteComp))
                continue;

            spriteComp.Visible = true;
        }

        _hiddenList.Clear();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_player.LocalEntity is null)
            return;

        var playerUid = _player.LocalEntity.Value;

        if (!_fovQuery.TryComp(playerUid, out var fovComp) ||
            !_xformQuery.TryComp(playerUid, out var xform) ||
            xform.MapUid == null)
            return;

        HashSet<EntityUid> lookup = new();
        var mapPos = _xform.GetMapCoordinates(playerUid);

        // Find all entities on every layers.
        // TODO: I think need some optimizations there
        //if (xform.MapUid is not null &&
        //    _zStack.TryGetZStack(xform.MapUid.Value, out var trackerComp))
        //{
        //    foreach (var mapUid in trackerComp.Value.Comp.Maps)
        //    {
        //        if (!_mapQuery.TryComp(mapUid, out var mapComp))
        //            continue;
//
        //        _lookup.GetEntitiesInRange(mapComp.MapId, mapPos.Position, fovComp.MaxDistance, lookup, LookupFlags.Dynamic | LookupFlags.Sundries);
        //    }
        //}
        //else
        //{
            lookup = _lookup.GetEntitiesInRange(playerUid, fovComp.MaxDistance, LookupFlags.Dynamic | LookupFlags.Sundries);
        //}

        //TODO: LookupFlags.Dynamic should also use some addional flags, because
        // corpses can moved or not, and we can't to take they into vision

        foreach (var ent in lookup)
        {
            // Should not hide player.
            if (ent == playerUid)
                continue;

            if (//!_mobMoverQuery.TryComp(ent, out var moverComp) ||
                !_spriteQuery.TryComp(ent, out var sprite))
            {
                if (_hiddenList.Contains(ent))
                    _hiddenList.Remove(ent);
                continue;
            }

            var result = IsInFieldOfView(
                playerUid,
                ent,
                fovComp.Angle,
                fovComp.MaxDistance,
                fovComp.GetRotation(fovComp.Direction),
                simple4DirMode: fovComp.Simple4DirMode);

            if (fovComp.Enabled)
            {
                if (!result && !_hiddenList.Contains(ent))
                    _hiddenList.Add(ent);
                else if (result && _hiddenList.Contains(ent))
                    _hiddenList.Remove(ent);
                sprite.Visible = result;
            }
            else
            {
                if (_hiddenList.Contains(ent))
                    _hiddenList.Remove(ent);
                sprite.Visible = true;
            }
        }
    }

    /// GENERATED BY DEEPSEEK BECUASE IM LAZY
    public bool IsInFieldOfView(
            EntityUid viewer,
            EntityUid target,
            float fovAngle,
            float maxDistance,
            float offsetAngle = -90f,
            bool simple4DirMode = false,
            TransformComponent? viewerTransform = null,
            TransformComponent? targetTransform = null,
            EyeComponent? eyeComp = null)
    {
        if (!Resolve(viewer, ref viewerTransform) ||
            !Resolve(target, ref targetTransform) ||
            !Resolve(viewer, ref eyeComp))
            return false;

        // Вычисляем направление от наблюдателя к цели
        var direction = targetTransform.WorldPosition - viewerTransform.WorldPosition;

        // Проверяем расстояние
        if (direction.LengthSquared() > maxDistance * maxDistance)
            return false;

        // Вычисляем угол между направлением взгляда наблюдателя и направлением на цель
        var viewerAngle = viewerTransform.WorldRotation; // + eyeComp.Rotation; // Угол поворота наблюдателя
        viewerAngle = viewerAngle.Reduced().FlipPositive();

        if (simple4DirMode)
        {
            var rsiDirection = SpriteComponent.Layer.GetDirection(Robust.Shared.Graphics.RSI.RsiDirectionType.Dir4, viewerAngle);
            var targetDirection = rsiDirection.Convert();
            viewerAngle = targetDirection.ToAngle();
        }

        var angleToTarget = Math.Atan2(direction.Y, direction.X); // Угол до цели

        // Нормализуем углы в диапазоне [0, 2π)
        viewerAngle = NormalizeAngle(viewerAngle);
        angleToTarget = NormalizeAngle(angleToTarget);

        // Добавляем смещение (offsetAngle) к углу взгляда наблюдателя
        float offsetAngleRadians = MathHelper.DegreesToRadians(offsetAngle);
        viewerAngle += offsetAngleRadians;

        // Нормализуем угол после добавления смещения
        viewerAngle = NormalizeAngle(viewerAngle);

        // Вычисляем разницу между углами
        var angleDifference = Math.Abs(viewerAngle - angleToTarget);

        // Если разница больше π, корректируем её
        if (angleDifference > Math.PI)
            angleDifference = 2 * Math.PI - angleDifference;

        // Переводим fovAngle из градусов в радианы
        float fovAngleRadians = MathHelper.DegreesToRadians(fovAngle);

        // Проверяем, попадает ли цель в поле зрения
        return angleDifference <= fovAngleRadians / 2;
    }

    private static double NormalizeAngle(double angle)
    {
        angle = (angle % (2 * Math.PI)); // Приводим угол к диапазону [0, 2π)
        if (angle < 0)
            angle += 2 * Math.PI; // Если угол отрицательный, добавляем 2π
        return angle;
    }
}
