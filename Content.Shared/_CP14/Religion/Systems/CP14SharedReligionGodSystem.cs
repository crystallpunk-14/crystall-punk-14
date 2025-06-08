using System.Numerics;
using Content.Shared._CP14.Religion.Components;
using Content.Shared.Interaction;
using Content.Shared.Verbs;

namespace Content.Shared._CP14.Religion.Systems;

public abstract partial class CP14SharedReligionGodSystem : EntitySystem
{

    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private EntityQuery<CP14ReligionObserverComponent> _observersQuery;

    public override void Initialize()
    {
        base.Initialize();

        _observersQuery = GetEntityQuery<CP14ReligionObserverComponent>();

        SubscribeLocalEvent<CP14ReligionVisionComponent, AccessibleOverrideEvent>(OnGodAccessible);
        SubscribeLocalEvent<CP14ReligionVisionComponent, InRangeOverrideEvent>(OnGodInRange);
        SubscribeLocalEvent<CP14ReligionVisionComponent, MenuVisibilityEvent>(OnGodMenu);
    }

    private void OnGodAccessible(Entity<CP14ReligionVisionComponent> ent, ref AccessibleOverrideEvent args)
    {

    }

    private void OnGodInRange(Entity<CP14ReligionVisionComponent> ent, ref InRangeOverrideEvent args)
    {
        args.Handled = true;
        
        args.InRange = InVision(args.Target, ent);
    }

    private void OnGodMenu(Entity<CP14ReligionVisionComponent> ent, ref MenuVisibilityEvent args)
    {
        args.Visibility &= ~MenuVisibility.NoFov;
    }

    protected bool InVision(EntityUid target, Entity<CP14ReligionVisionComponent> user)
    {
        var userXform = Transform(user);
        var query = EntityQueryEnumerator<CP14ReligionObserverComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var observer, out var xform))
        {
            if (!observer.Active)
                continue;

            if (xform.MapID != userXform.MapID)
                continue;

            if (user.Comp.Religion is not null & user.Comp.Religion != observer.Religion)
                continue;

            var obsPos = _transform.GetWorldPosition(uid);
            var targetPos = _transform.GetWorldPosition(target);
            if (Vector2.Distance(obsPos, targetPos) <= observer.Range)
            {
                // If the observer is within range of the target, they can see it.
                return true;
            }
        }

        return false;
    }
}
