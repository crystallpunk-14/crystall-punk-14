using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Stacks;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Wallpaper;

public partial class CP14SharedWallpaperSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedStackSystem _stack = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14WallpaperHolderComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<CP14WallpaperHolderComponent, CP14WallpaperAddLayerDoAfterEvent>(OnAddDoAfter);
        SubscribeLocalEvent<CP14WallpaperHolderComponent, CP14WallpaperRemoveLayersDoAfterEvent>(OnRemoveDoAfter);
    }

    private void OnRemoveDoAfter(Entity<CP14WallpaperHolderComponent> holder, ref CP14WallpaperRemoveLayersDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Args.Target == null)
            return;

        holder.Comp.Layers.Clear();
        Dirty(holder);
    }

    private void OnAddDoAfter(Entity<CP14WallpaperHolderComponent> holder, ref CP14WallpaperAddLayerDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Args.Target == null)
            return;

        if (!TryComp<CP14WallpaperComponent>(args.Used, out var wallpaper))
            return;

        var pos1 = _transform.GetWorldPosition(args.User);
        var pos2 = _transform.GetWorldPosition(holder);

        var deltaX = pos2.X - pos1.X;
        var deltaY = pos2.Y - pos1.Y;

        string direction;

        if (Math.Abs(deltaX) > Math.Abs(deltaY))
            direction = deltaX > 0 ? "Right" : "Left";
        else
            direction = deltaY > 0 ? "Bottom" : "Top";

        //TODO: is incorrectly calculated if the wall is turned the wrong way. Temporarily fixed by adding Transform noRot: true

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

        //Remove item
        if (TryComp<StackComponent>(args.Used, out var stack))
        {
            _stack.SetCount(args.Used.Value, stack.Count - 1, stack);
        }
        else
        {
            QueueDel(args.Used);
        }
    }

    private void OnInteractUsing(Entity<CP14WallpaperHolderComponent> holder, ref InteractUsingEvent args)
    {
        if (TryComp<CP14WallpaperComponent>(args.Used, out var wallpaper))
        {
            var doAfterArgs = new DoAfterArgs(EntityManager, args.User, wallpaper.Delay, new CP14WallpaperAddLayerDoAfterEvent(), holder, holder, args.Used)
            {
                BreakOnDamage = true,
                BreakOnMove = true,
                MovementThreshold = 0.5f,
                CancelDuplicate = false,
            };

            _doAfter.TryStartDoAfter(doAfterArgs);
            return;
        }

        if (TryComp<CP14WallpaperRemoverComponent>(args.Used, out var remover))
        {
            if (holder.Comp.Layers.Count == 0)
                return;

            var doAfterArgs = new DoAfterArgs(EntityManager, args.User, remover.Delay, new CP14WallpaperRemoveLayersDoAfterEvent(), holder, holder, args.Used)
            {
                BreakOnDamage = true,
                BreakOnMove = true,
                MovementThreshold = 0.5f,
                CancelDuplicate = false,
            };

            _doAfter.TryStartDoAfter(doAfterArgs);
            return;
        }
    }
}

[Serializable, NetSerializable]
public sealed partial class CP14WallpaperAddLayerDoAfterEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class CP14WallpaperRemoveLayersDoAfterEvent : SimpleDoAfterEvent
{
}
