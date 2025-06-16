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

    public void EditObservation(EntityUid target, ProtoId<CP14ReligionPrototype> religion, float range)
    {
        EnsureComp<CP14ReligionObserverComponent>(target, out var observer);

        if (observer.Observation.ContainsKey(religion))
        {
            var newRange = Math.Clamp(observer.Observation[religion] + range, 0, float.MaxValue);

            if (newRange <= 0)
                observer.Observation.Remove(religion);
            else
                observer.Observation[religion] = newRange;
        }
        else
        {
            // Otherwise, add a new observation for the religion.
            observer.Observation.Add(religion, range);
        }

        Dirty(target, observer);
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

        var userXform = Transform(user);
        var query = EntityQueryEnumerator<CP14ReligionObserverComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var observer, out var xform))
        {
            if (!observer.Active)
                continue;

            if (xform.MapID != userXform.MapID)
                continue;

            if (user.Comp.Religion is null)
                continue;

            if (!observer.Observation.ContainsKey(user.Comp.Religion.Value))
                continue;


            var obsPos = _transform.GetWorldPosition(uid);
            var targetPos = coords.Position;
            if (Vector2.Distance(obsPos, targetPos) <= observer.Observation[user.Comp.Religion.Value])
            {
                // If the observer is within range of the target, they can see it.
                return true;
            }
        }

        return false;
    }
}
