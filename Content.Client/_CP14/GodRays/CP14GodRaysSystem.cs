using Robust.Client.GameObjects;
using Robust.Shared.Map.Components;

namespace Content.Client._CP14.GodRays;

public sealed partial class CP14GodRaysSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly PointLightSystem _pointLight = default!;

    private EntityQuery<MapLightComponent> _mapLightQuery;

    public override void Initialize()
    {
        base.Initialize();

        _mapLightQuery = GetEntityQuery<MapLightComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var spriteSync = EntityQueryEnumerator<CP14SyncColorWithMapLightComponent>();
        while (spriteSync.MoveNext(out var uid, out var syncComp))
        {
            if (!_mapLightQuery.TryComp(Transform(uid).MapUid, out var map))
                continue;

            //We calculate target color as map color, but transparency is based on map color brightness
            var targetColor = Color.ToHsv(map.AmbientLightColor);
            targetColor.W = targetColor.Z / 2;

            var finalColor = Color.FromHsv(targetColor);

            _sprite.SetColor(uid, finalColor);
            _pointLight.SetColor(uid, finalColor);
        }
    }
}
