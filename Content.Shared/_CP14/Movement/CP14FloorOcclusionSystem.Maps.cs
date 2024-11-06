using Content.Shared.Movement.Components;

namespace Content.Shared.Movement.Systems;

public abstract partial class SharedFloorOcclusionSystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    private void CP14InitializeMapOccluder()
    {
        SubscribeLocalEvent<FloorOcclusionComponent, EntParentChangedMessage>(OnParentChanged);
    }

    private void OnParentChanged(Entity<FloorOcclusionComponent> ent, ref EntParentChangedMessage args)
    {
        if (args.OldParent == null || TerminatingOrDeleted(ent))
            return;

        if (ent.Comp.Colliding.Contains(args.OldParent.Value))
            ent.Comp.Colliding.Remove(args.OldParent.Value);

        var newParent = _transform.GetParentUid(ent);
        if (HasComp<CP14MapFloorOccluderComponent>(newParent))
        {
            if (!ent.Comp.Colliding.Contains(newParent))
                ent.Comp.Colliding.Add(newParent);
        }

        Dirty(ent);
        SetEnabled(ent);
    }
}
