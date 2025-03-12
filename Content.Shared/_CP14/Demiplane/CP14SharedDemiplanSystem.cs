using Content.Shared._CP14.Demiplane.Components;
using Content.Shared._CP14.DemiplaneTraveling;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Demiplane;

public abstract partial  class CP14SharedDemiplaneSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14DemiplaneRiftOpenedComponent, InteractHandEvent>(OnDemiplanePasswayInteract);
        SubscribeLocalEvent<CP14DemiplaneHintComponent, MapInitEvent>(OnDemiplaneHintMapInit);
    }

    private void OnDemiplaneHintMapInit(Entity<CP14DemiplaneHintComponent> ent, ref MapInitEvent args)
    {
        var query = EntityQueryEnumerator<CP14DemiplaneRiftOpenedComponent, TransformComponent>();

        var xformHint = Transform(ent);
        var hintPos = _transform.GetWorldPosition(xformHint);

        while (query.MoveNext(out _, out _, out var xformRift))
        {
            if (xformRift.MapUid != xformHint.MapUid)
                continue;

            var riftPos = _transform.GetWorldPosition(xformRift);

            //Calculate the rotation
            Angle angle = new(riftPos - hintPos);

            _transform.SetWorldRotation(ent, angle + Angle.FromDegrees(90));
            break;
        }
    }

    private void OnDemiplanePasswayInteract(Entity<CP14DemiplaneRiftOpenedComponent> passway, ref InteractHandEvent args)
    {
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager,
            args.User,
            passway.Comp.DoAfter,
            new CP14DemiplanPasswayUseDoAfter(),
            args.Target,
            args.Target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnHandChange = true,
            NeedHand = true,
            MovementThreshold = 0.2f,
        });
    }

    public virtual bool TryTeleportIntoDemiplane(Entity<CP14DemiplaneComponent> demiplane, EntityUid? entity)
    {
        return true;
    }

    public virtual bool TryTeleportOutDemiplane(Entity<CP14DemiplaneComponent> demiplane, EntityUid? entity)
    {
        return true;
    }
}

[Serializable, NetSerializable]
public sealed partial class CP14DemiplanPasswayUseDoAfter : SimpleDoAfterEvent
{
}
