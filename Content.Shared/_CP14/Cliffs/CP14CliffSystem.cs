using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._CP14.Cliffs;

public partial class CP14CliffSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14CliffComponent, StartCollideEvent>(OnStartCollide);
    }

    private void OnStartCollide(Entity<CP14CliffComponent> cliff, ref StartCollideEvent args)
    {
        if (cliff.Comp.TriggerFallFixtureId != args.OurFixtureId)
            return;

        if (!TryComp<PhysicsComponent>(args.OtherEntity, out var physics))
            return;

        _stun.TryParalyze(args.OtherEntity, cliff.Comp.ParalyzeTime, true);
        _audio.PlayPredicted(cliff.Comp.FallSound, args.OtherEntity, args.OtherEntity);

        var offset = (Transform(cliff).LocalRotation + cliff.Comp.fallDirection).ToWorldVec();
        var targetPos = _transform.GetWorldPosition(cliff) + offset;
        _transform.SetWorldPosition(args.OtherEntity, targetPos);

        var velocity =  physics.LinearVelocity * cliff.Comp.LaunchForwardsMultiplier;
        _physics.SetLinearVelocity(args.OtherEntity, velocity, body: physics);
    }
}
