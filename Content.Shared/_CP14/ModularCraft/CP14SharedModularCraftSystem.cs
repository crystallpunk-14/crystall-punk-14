using Content.Shared._CP14.ModularCraft.Components;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Labels.EntitySystems;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.ModularCraft;

public abstract class CP14SharedModularCraftSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly LabelSystem _label = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14ModularCraftStartPointComponent, AfterInteractEvent>(OnAfterInteractStart);
        SubscribeLocalEvent<CP14ModularCraftPartComponent, AfterInteractEvent>(OnAfterInteractPart);
        SubscribeLocalEvent<CP14LabeledRenamingComponent, CP14LabeledEvent>(OnLabelRenaming);
    }

    private void OnLabelRenaming(Entity<CP14LabeledRenamingComponent> ent, ref CP14LabeledEvent args)
    {
        if (args.Text is null)
            return;
        _meta.SetEntityName(ent, args.Text);
        _label.Label(ent, null);
    }

    private void OnAfterInteractStart(Entity<CP14ModularCraftStartPointComponent> start, ref AfterInteractEvent args)
    {
        if (args.Handled || args.Target is null)
            return;

        if (!TryComp<CP14ModularCraftPartComponent>(args.Target, out var part))
            return;

        var xform = Transform(args.Target.Value);
        if (xform.GridUid != xform.ParentUid)
            return;

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager,
            args.User,
            part.DoAfter,
            new CP14ModularCraftAddPartDoAfter(),
            args.Target,
            args.Target,
            start)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnDropItem = true,
        });

        args.Handled = true;
    }

    private void OnAfterInteractPart(Entity<CP14ModularCraftPartComponent> part, ref AfterInteractEvent args)
    {
        if (args.Handled || args.Target is null)
            return;

        if (!HasComp<CP14ModularCraftStartPointComponent>(args.Target))
            return;

        var xform = Transform(args.Target.Value);
        if (xform.GridUid != xform.ParentUid)
            return;

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager,
            args.User,
            part.Comp.DoAfter,
            new CP14ModularCraftAddPartDoAfter(),
            args.Target,
            args.Target,
            part)
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
