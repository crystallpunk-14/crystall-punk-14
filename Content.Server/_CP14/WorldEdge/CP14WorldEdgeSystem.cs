using System.Numerics;
using Content.Server.Chat.Managers;
using Content.Server.Database;
using Content.Shared._CP14.WorldEdge;
using Content.Shared.Administration.Logs;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.Mind.Components;
using Content.Shared.Physics;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._CP14.WorldEdge;

public sealed class CP14WorldEdgeSystem : CP14SharedWorldEdgeSystem
{
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] protected readonly ISharedAdminLogManager AdminLog = default!;
    [Dependency] private readonly FixtureSystem _fixtures = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CP14WorldEdgeComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<CP14WorldBoundingComponent, StartCollideEvent>(OnWorldEdgeCollide);
        SubscribeLocalEvent<CP14WorldRemovePendingComponent, EntityUnpausedEvent>(OnPendingUnpaused);
    }

    private void OnPendingUnpaused(Entity<CP14WorldRemovePendingComponent> ent, ref EntityUnpausedEvent args)
    {
        ent.Comp.RemoveTime += args.PausedTime;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14WorldRemovePendingComponent>();
        while (query.MoveNext(out var uid, out var pending))
        {
            if (pending.RemoveTime >= _timing.CurTime)
                continue;

            if (Paused(uid))
                continue;

            if (pending.Bounding == null)
            {
                CancelRemoving((uid, pending));
                continue;
            }

            var entPos = _transform.GetWorldPosition(uid);
            var originPos = _transform.GetWorldPosition(pending.Bounding.Value);
            var distance = Vector2.Distance(entPos, originPos);

            if (distance > pending.Bounding.Value.Comp.Range)
            {
                RoundRemoveMind((uid, pending));
            }
            else
            {
                CancelRemoving((uid, pending));
            }
        }
    }

    private void RoundRemoveMind(Entity<CP14WorldRemovePendingComponent> ent)
    {
        AdminLog.Add(
            LogType.Action,
            LogImpact.High,
            $"{ToPrettyString(ent):player} has left the playing area, and is out of the round.");

        QueueDel(ent);
    }

    private void CancelRemoving(Entity<CP14WorldRemovePendingComponent> ent)
    {
        RemComp<CP14WorldRemovePendingComponent>(ent);

        if (TryComp<ActorComponent>(ent, out var actor))
        {
            var msg = Loc.GetString("cp14-world-edge-cancel-removing-message");
            _chatManager.ChatMessageToOne(ChatChannel.Server, msg, msg, ent, false, actor.PlayerSession.Channel);
        }
    }

    private void OnWorldEdgeCollide(Entity<CP14WorldBoundingComponent> bounding, ref StartCollideEvent args)
    {
        if (!TryComp<MindContainerComponent>(args.OtherEntity, out var mindContainer))
            return;

        if (TryComp<ActorComponent>(args.OtherEntity, out var actor) &&
            !HasComp<CP14WorldRemovePendingComponent>(args.OtherEntity))
        {
            var msg = Loc.GetString("cp14-world-edge-pre-remove-message",
                ("second", bounding.Comp.ReturnTime.TotalSeconds));
            _chatManager.ChatMessageToOne(ChatChannel.Server, msg, msg, args.OtherEntity, false, actor.PlayerSession.Channel);
        }

        var removePending = EnsureComp<CP14WorldRemovePendingComponent>(args.OtherEntity);
        removePending.RemoveTime = _timing.CurTime + bounding.Comp.ReturnTime;
        removePending.Bounding = bounding;

    }

    private void OnMapInit(Entity<CP14WorldEdgeComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.BoundaryEntity = CreateBoundary(new EntityCoordinates(ent, ent.Comp.Origin), ent.Comp.Range);
    }

    public EntityUid CreateBoundary(EntityCoordinates coordinates, float range)
    {
        var boundaryUid = Spawn(null, coordinates);
        var boundaryPhysics = AddComp<PhysicsComponent>(boundaryUid);
        var cShape = new ChainShape();
        // Don't need it to be a perfect circle, just need it to be loosely accurate.
        cShape.CreateLoop(Vector2.Zero, range + 0.25f, false, 4);
        _fixtures.TryCreateFixture(
            boundaryUid,
            cShape,
            "boundary",
            collisionLayer: (int) (CollisionGroup.HighImpassable | CollisionGroup.Impassable | CollisionGroup.LowImpassable),
            body: boundaryPhysics,
            hard: false);

        _physics.WakeBody(boundaryUid, body: boundaryPhysics);
        var bounding = AddComp<CP14WorldBoundingComponent>(boundaryUid);
        bounding.Range = range + 0.25f;
        return boundaryUid;
    }
}
