using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Tools.Components;
using Content.Shared.Tools.Systems;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.DestroyedByTool;

public sealed partial class CP14DestroyedByToolSystem : EntitySystem
{
    [Dependency] private readonly SharedToolSystem _tool = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14DestroyedByToolComponent, CP14DestroyedByToolDoAfterEvent>(OnDestroyDoAfter);
        SubscribeLocalEvent<CP14DestroyedByToolComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteractUsing(Entity<CP14DestroyedByToolComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.Tool == null || !_tool.HasQuality(args.Used, ent.Comp.Tool))
            return;

        if (TryComp<ToolComponent>(args.Used, out var tool))
        {
            _tool.PlayToolSound(args.Used, tool, args.User);
        }

        var doAfterArgs =
            new DoAfterArgs(EntityManager, args.User, ent.Comp.RemoveTime, new CP14DestroyedByToolDoAfterEvent(), args.Target)
            {
                BreakOnDamage = true,
                BlockDuplicate = true,
                BreakOnMove = true,
                BreakOnHandChange = true,
            };
        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnDestroyDoAfter(Entity<CP14DestroyedByToolComponent> ent, ref CP14DestroyedByToolDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        QueueDel(ent);

        args.Handled = true;
    }
}

[Serializable, NetSerializable]
public sealed partial class CP14DestroyedByToolDoAfterEvent : SimpleDoAfterEvent
{
}
