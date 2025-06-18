using Content.Shared.ActionBlocker;
using Content.Shared.Movement.Events;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;

namespace Content.Shared._CP14.Dash;

public sealed partial class CP14DashSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14DashComponent, UpdateCanMoveEvent>(OnMoveAttempt);
        SubscribeLocalEvent<CP14DashComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<CP14DashComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<CP14DashComponent, LandEvent>(OnLand);
    }

    private void OnShutdown(Entity<CP14DashComponent> ent, ref ComponentShutdown args)
    {
        _blocker.UpdateCanMove(ent);
    }

    private void OnInit(Entity<CP14DashComponent> ent, ref ComponentInit args)
    {
        _blocker.UpdateCanMove(ent);
    }

    private void OnLand(Entity<CP14DashComponent> ent, ref LandEvent args)
    {
        RemCompDeferred<CP14DashComponent>(ent);
    }

    private void OnMoveAttempt(Entity<CP14DashComponent> ent, ref UpdateCanMoveEvent args)
    {
        if (ent.Comp.LifeStage > ComponentLifeStage.Running)
            return;

        //Cant move while dashing
        args.Cancel();
    }

    public void PerformDash(EntityUid ent, EntityCoordinates targetPosition, float speed = 10f)
    {
        EnsureComp<CP14DashComponent>(ent, out var dash);
        _audio.PlayPredicted(dash.DashSound, ent, ent);
        _throwing.TryThrow(ent, targetPosition, speed, null, 0f, 10, true, false, false, false, false);
    }
}
