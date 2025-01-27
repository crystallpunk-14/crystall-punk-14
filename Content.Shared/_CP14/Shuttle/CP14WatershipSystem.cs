using Content.Shared.Construction.Components;
using Content.Shared.Construction.EntitySystems;
using Content.Shared.Verbs;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._CP14.Shuttle;

public sealed class CP14WatershipSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CP14WaterShipPaddleComponent, GetVerbsEvent<ActivationVerb>>(OnPaddleForward);
        SubscribeLocalEvent<CP14WaterShipPaddleComponent, GetVerbsEvent<AlternativeVerb>>(OnPaddleBackward);
    }

    private void OnPaddleForward(Entity<CP14WaterShipPaddleComponent> ent, ref GetVerbsEvent<ActivationVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        var paddleTransform = Transform(ent);

        if (!paddleTransform.Anchored)
            return;

        var verb = new ActivationVerb
        {
            Text = "Forward",
            Priority = 1,
            Act = () => Bulb(ent, paddleTransform, 1)
        };
        args.Verbs.Add(verb);
    }

    private void OnPaddleBackward(Entity<CP14WaterShipPaddleComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        var paddleTransform = Transform(ent);

        if (!paddleTransform.Anchored)
            return;

        var verb = new AlternativeVerb
        {
            Text = "Backward",
            Priority = 2,
            Act = () => Bulb(ent, paddleTransform, -1)
        };
        args.Verbs.Add(verb);
    }

    private void Bulb(Entity<CP14WaterShipPaddleComponent> ent, TransformComponent paddleTransform, float modifier)
    {
        if (paddleTransform.GridUid is null)
            return;

        var direction = _transform.GetWorldRotation(paddleTransform) + ent.Comp.ImpulseAngle;
        var impulseDirection = direction.ToVec();

        _physics.ApplyLinearImpulse(paddleTransform.GridUid.Value, impulseDirection * ent.Comp.Power * modifier, paddleTransform.LocalPosition);
    }
}
