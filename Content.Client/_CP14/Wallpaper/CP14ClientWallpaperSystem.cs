using Content.Shared._CP14.Wallpaper;
using Robust.Client.GameObjects;

namespace Content.Client._CP14.Wallpaper;

public sealed class CP14ClientWallpaperSystem : CP14SharedWallpaperSystem
{
    public override void Initialize()
    {

        SubscribeLocalEvent<CP14WallpaperHolderComponent, AfterAutoHandleStateEvent>(OnHandleState);
    }

    private void OnHandleState(Entity<CP14WallpaperHolderComponent> holder, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp<SpriteComponent>(holder, out var sprite))
            return;

        //Remove old layers
        foreach (var key in holder.Comp.RevealedLayers)
        {
            sprite.RemoveLayer(key);
        }
        holder.Comp.RevealedLayers.Clear();

        //Add new layers
        var counter = 0;
        foreach (var wallpaper in holder.Comp.Layers)
        {
            var keyCode = $"wallpaper-layer-{counter}";
            holder.Comp.RevealedLayers.Add(keyCode);

            var index = sprite.LayerMapReserveBlank(keyCode);

            sprite.LayerSetSprite(index, wallpaper);
            counter++;
        }
    }
}
