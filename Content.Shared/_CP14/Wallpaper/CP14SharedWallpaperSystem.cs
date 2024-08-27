using System.Diagnostics;
using Content.Shared.Interaction;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Wallpaper;

public partial class CP14SharedWallpaperSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14WallpaperHolderComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteractUsing(Entity<CP14WallpaperHolderComponent> holder, ref InteractUsingEvent args)
    {
        if (!TryComp<CP14WallpaperComponent>(args.Used, out var wallpaper))
            return;

        var pos1 = _transform.GetWorldPosition(args.User);
        var pos2 = _transform.GetWorldPosition(holder);

        // Рассчитываем разницу в координатах
        var deltaX = pos2.X - pos1.X;
        var deltaY = pos2.Y - pos1.Y;

        // Определяем направление
        string direction;

        if (Math.Abs(deltaX) > Math.Abs(deltaY))
            direction = deltaX > 0 ? "Right" : "Left";
        else
            direction = deltaY > 0 ? "Bottom" : "Top";

        switch (direction)
        {
            case "Bottom":
                holder.Comp.Layers.Add(new SpriteSpecifier.Rsi(new ResPath(wallpaper.RsiPath), wallpaper.Bottom));
                break;
            case "Top":
                holder.Comp.Layers.Add(new SpriteSpecifier.Rsi(new ResPath(wallpaper.RsiPath), wallpaper.Top));
                break;
            case "Left":
                holder.Comp.Layers.Add(new SpriteSpecifier.Rsi(new ResPath(wallpaper.RsiPath), wallpaper.Left));
                break;
            case "Right":
                holder.Comp.Layers.Add(new SpriteSpecifier.Rsi(new ResPath(wallpaper.RsiPath), wallpaper.Right));
                break;
        }

        Dirty(holder);
    }
}
