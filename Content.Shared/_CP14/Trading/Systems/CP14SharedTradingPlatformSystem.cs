using Content.Shared._CP14.Trading.Components;
using Content.Shared._CP14.Trading.Prototypes;
using Content.Shared.UserInterface;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Trading.Systems;

public abstract partial class CP14SharedTradingPlatformSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();


        SubscribeLocalEvent<CP14TradingPlatformComponent, CP14TradingPositionUnlockAttempt>(OnUnlockAttempt);

        SubscribeLocalEvent<CP14TradingPlatformComponent, BeforeActivatableUIOpenEvent>(OnBeforeUIOpen);
        SubscribeLocalEvent<CP14TradingReputationComponent, MapInitEvent>(OnReputationMapInit);
    }

    private void OnUnlockAttempt(Entity<CP14TradingPlatformComponent> ent, ref CP14TradingPositionUnlockAttempt args)
    {
        TryUnlockPosition(args.Actor, args.Position);
    }

    private void OnReputationMapInit(Entity<CP14TradingReputationComponent> ent, ref MapInitEvent args)
    {
        foreach (var faction in _proto.EnumeratePrototypes<CP14TradingFactionPrototype>())
        {
            ent.Comp.Reputation[faction] = ent.Comp.Reputation.GetValueOrDefault(faction, 0f) + ent.Comp.GlobalRoundstartReputation;
        }
    }

    private void OnBeforeUIOpen(Entity<CP14TradingPlatformComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        if (!TryComp<CP14TradingReputationComponent>(args.User, out var repComp))
            return;

        _userInterface.SetUiState(ent.Owner, CP14TradingUiKey.Key, new CP14TradingPlatformUiState(repComp.Reputation, repComp.UnlockedPositions));
    }

    public bool TryUnlockPosition(Entity<CP14TradingReputationComponent?> user, ProtoId<CP14TradingPositionPrototype> position)
    {
        if (!Resolve(user.Owner, ref user.Comp, false))
            return false;

        if (!_proto.TryIndex(position, out var indexedPosition))
            return false;

        if (user.Comp.UnlockedPositions.Contains(position))
            return false;

        if (indexedPosition.Prerequisite is not null && !user.Comp.UnlockedPositions.Contains(indexedPosition.Prerequisite.Value))
            return false;

        if (user.Comp.Reputation.GetValueOrDefault(indexedPosition.Faction, 0f) < indexedPosition.UnlockCost)
            return false;

        user.Comp.Reputation[indexedPosition.Faction] -= indexedPosition.UnlockCost;
        user.Comp.UnlockedPositions.Add(position);
        Dirty(user);

        return true;
    }
}

[Serializable, NetSerializable]
public sealed class CP14TradingPositionUnlockAttempt(ProtoId<CP14TradingPositionPrototype> position) : BoundUserInterfaceMessage
{
    public readonly ProtoId<CP14TradingPositionPrototype> Position = position;
}
