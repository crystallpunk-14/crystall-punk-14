using Content.Shared._CP14.Demiplane.Components;
using Content.Shared._CP14.DemiplaneTraveling;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Robust.Shared.Player;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Demiplane;

public abstract partial  class CP14SharedDemiplaneSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14DemiplaneRiftOpenedComponent, InteractHandEvent>(OnDemiplanPasswayInteract);

        SubscribeLocalEvent<CP14DemiplaneDestroyWithoutPlayersComponent, CP14DemiplanEntityEnterEvent>(OnEntityEnter);
        SubscribeLocalEvent<CP14DemiplaneDestroyWithoutPlayersComponent, CP14DemiplanEntityLeaveEvent>(OnEntityLeave);
    }

    private void OnEntityLeave(Entity<CP14DemiplaneDestroyWithoutPlayersComponent> ent, ref CP14DemiplanEntityLeaveEvent args)
    {
        //это можно легко абузить, если игроки найдут способ выходить из
        //демиплана другим способом. Лучше добавлять на игроков компонент, и тречить все смены карт у этого компонента
        if (ent.Comp.Players.Contains(args.Player))
        {
            ent.Comp.Players.Remove(args.Player);
            if (ent.Comp.Players.Count == 0)
            {
                QueueDel(ent);
            }
        }
    }

    private void OnEntityEnter(Entity<CP14DemiplaneDestroyWithoutPlayersComponent> ent, ref CP14DemiplanEntityEnterEvent args)
    {
        if (!TryComp<ActorComponent>(args.Player, out var actor))
            return;

        ent.Comp.Players.Add(args.Player);
    }

    private void OnDemiplanPasswayInteract(Entity<CP14DemiplaneRiftOpenedComponent> passway, ref InteractHandEvent args)
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
}

[Serializable, NetSerializable]
public sealed partial class CP14DemiplanPasswayUseDoAfter : SimpleDoAfterEvent
{
}

/// <summary>
/// Is invoked on the demiplane when new players enter. This only applies to rift entrances, any other methods will not be taken into account.
/// </summary>
public sealed class CP14DemiplanEntityEnterEvent : EntityEventArgs
{
    public EntityUid Player;

    public CP14DemiplanEntityEnterEvent(EntityUid player)
    {
        Player = player;
    }
}

/// <summary>
/// Is invoked on the demiplane when some players left. This only applies to rift entrances, any other methods will not be taken into account.
/// </summary>
public sealed class CP14DemiplanEntityLeaveEvent : EntityEventArgs
{
    public EntityUid Player;

    public CP14DemiplanEntityLeaveEvent(EntityUid player)
    {
        Player = player;
    }
}
