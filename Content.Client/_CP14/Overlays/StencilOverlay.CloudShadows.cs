using System.Numerics;
using Content.Shared._CP14.CloudShadow;
using Robust.Client.Graphics;
using Robust.Shared.Utility;

namespace Content.Client.Overlays;

public sealed partial class StencilOverlay
{
    private void DrawCloudShadows(in OverlayDrawArgs args, CP14CloudShadowsComponent cloudComp, Matrix3x2 invMatrix)
    {
        var worldHandle = args.WorldHandle;
        var mapId = args.MapId;
        var worldAABB = args.WorldAABB;
        var worldBounds = args.WorldBounds;
        var position = args.Viewport.Eye?.Position.Position ?? Vector2.Zero;

        // Cut out the irrelevant bits via stencil
        // This is why we don't just use parallax; we might want specific tiles to get drawn over
        // particularly for planet maps or stations.
        worldHandle.RenderInRenderTarget(_blep!, () =>
        {
            var xformQuery = _entManager.GetEntityQuery<TransformComponent>();
            _grids.Clear();

            // idk if this is safe to cache in a field and clear sloth help
            _mapManager.FindGridsIntersecting(mapId, worldAABB, ref _grids);

            foreach (var grid in _grids)
            {
                var matrix = _transform.GetWorldMatrix(grid, xformQuery);
                var matty =  Matrix3x2.Multiply(matrix, invMatrix);
                worldHandle.SetTransform(matty);

                foreach (var tile in grid.Comp.GetTilesIntersecting(worldAABB))
                {
                    // Ignored tiles for stencil
                    if (_weather.CanWeatherAffect(grid.Owner, grid, tile))
                    {
                        continue;
                    }

                    var gridTile = new Box2(tile.GridIndices * grid.Comp.TileSize,
                        (tile.GridIndices + Vector2i.One) * grid.Comp.TileSize);

                    worldHandle.DrawRect(gridTile, Color.White);
                }
            }

        }, Color.Transparent);

        worldHandle.SetTransform(Matrix3x2.Identity);
        worldHandle.UseShader(_protoManager.Index<ShaderPrototype>("StencilMask").Instance());
        worldHandle.DrawTextureRect(_blep!.Texture, worldBounds);
        var curTime = _timing.RealTime;
        var sprite = _sprite.GetFrame(new SpriteSpecifier.Texture(cloudComp.ParallaxPath), curTime);

        // Draw the rain
        worldHandle.UseShader(_protoManager.Index<ShaderPrototype>("StencilDraw").Instance());
        _parallax.DrawParallax(worldHandle, worldAABB, sprite, curTime, position, cloudComp.CloudSpeed, modulate: Color.White.WithAlpha(cloudComp.Alpha), scale: cloudComp.Scale);

        worldHandle.SetTransform(Matrix3x2.Identity);
        worldHandle.UseShader(null);
    }
}
