using Content.Shared._CP14.ModularCraft.Components;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.ModularCraft;

public abstract class CP14SharedModularCraftSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14ModularCraftPartComponent, AfterInteractEvent>(OnAfterInteractPart);
    }

    private void OnAfterInteractPart(Entity<CP14ModularCraftPartComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || args.Target is null)
            return;

        if (!TryComp<CP14ModularCraftStartPointComponent>(args.Target, out var starterPoint))
            return;

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager,
            args.User,
            ent.Comp.DoAfter,
            new CP14ModularCraftAddPartDoAfter(),
            args.Target,
            args.Target,
            ent)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnDropItem = true,
        });

        args.Handled = true;
    }
}

[Serializable, NetSerializable]
public sealed partial class CP14ModularCraftAddPartDoAfter : SimpleDoAfterEvent
{
}
