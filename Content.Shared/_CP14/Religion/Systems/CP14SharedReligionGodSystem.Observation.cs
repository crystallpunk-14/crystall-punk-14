using System.Numerics;
using Content.Shared._CP14.Religion.Components;
using Content.Shared._CP14.Religion.Prototypes;
using Content.Shared.Interaction;
using Content.Shared.Verbs;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Religion.Systems;

public abstract partial class CP14SharedReligionGodSystem
{
    private void InitializeObservation()
    {
        SubscribeLocalEvent<CP14ReligionEntityComponent, InRangeOverrideEvent>(OnGodInRange);
        SubscribeLocalEvent<CP14ReligionEntityComponent, MenuVisibilityEvent>(OnGodMenu);
    }

    private void OnGodInRange(Entity<CP14ReligionEntityComponent> ent, ref InRangeOverrideEvent args)
    {
        args.Handled = true;

        args.InRange = InVision(args.Target, ent);
    }

    private void OnGodMenu(Entity<CP14ReligionEntityComponent> ent, ref MenuVisibilityEvent args)
    {
        args.Visibility &= ~MenuVisibility.NoFov;
    }

    public bool InVision(EntityUid target, Entity<CP14ReligionEntityComponent> user)
    {
        var position = Transform(target).Coordinates;

        return InVision(position, user);
    }

    public bool InVision(EntityCoordinates coords, Entity<CP14ReligionEntityComponent> user)
    {
        if (!HasComp<CP14ReligionVisionComponent>(user))
            return true;

        if (user.Comp.Religion is null)
            return true;

        var userXform = Transform(user);
        var query = EntityQueryEnumerator<CP14ReligionObserverComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var observer, out var xform))
        {
            if (!observer.Active || observer.Religion is null || observer.Radius <= 0f)
                continue;

            if (xform.MapID != userXform.MapID)
                continue;

            if (observer.Religion.Value != user.Comp.Religion.Value)
                continue;

            var obsPos = _transform.GetWorldPosition(uid);
            var targetPos = coords.Position;
            if (Vector2.Distance(obsPos, targetPos) <= observer.Radius)
            {
                return observer.Religion.Value == user.Comp.Religion.Value;
            }
        }

        return false;
    }
}
