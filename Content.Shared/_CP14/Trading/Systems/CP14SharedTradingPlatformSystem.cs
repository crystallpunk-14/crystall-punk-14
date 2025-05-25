using Content.Shared._CP14.Trading.Components;
using Content.Shared._CP14.Trading.Prototypes;
using Content.Shared.UserInterface;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.Trading.Systems;

public abstract partial class CP14SharedTradingPlatformSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14TradingReputationComponent, MapInitEvent>(OnReputationMapInit);

        SubscribeLocalEvent<CP14TradingPlatformComponent, CP14TradingPositionUnlockAttempt>(OnUnlockAttempt);
        SubscribeLocalEvent<CP14TradingPlatformComponent, CP14TradingPositionBuyAttempt>(OnBuyAttempt);

        SubscribeLocalEvent<CP14TradingPlatformComponent, BeforeActivatableUIOpenEvent>(OnBeforeUIOpen);
    }

    private void OnReputationMapInit(Entity<CP14TradingReputationComponent> ent, ref MapInitEvent args)
    {
        foreach (var faction in _proto.EnumeratePrototypes<CP14TradingFactionPrototype>())
        {
            ent.Comp.Reputation[faction] = ent.Comp.Reputation.GetValueOrDefault(faction, 0f) + ent.Comp.GlobalRoundstartReputation;
        }
        Dirty(ent);
    }

    private void OnBuyAttempt(Entity<CP14TradingPlatformComponent> ent, ref CP14TradingPositionBuyAttempt args)
    {
        TryBuyPosition(args.Actor, ent, args.Position);
        UpdateUIState(ent, args.Actor);
    }

    private void OnUnlockAttempt(Entity<CP14TradingPlatformComponent> ent, ref CP14TradingPositionUnlockAttempt args)
    {
        TryUnlockPosition(args.Actor, args.Position);
        UpdateUIState(ent, args.Actor);
    }

    private void OnBeforeUIOpen(Entity<CP14TradingPlatformComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        UpdateUIState(ent, args.User);
    }

    private void UpdateUIState(Entity<CP14TradingPlatformComponent> ent, EntityUid user)
    {
        if (!TryComp<CP14TradingReputationComponent>(user, out var repComp))
            return;

        _userInterface.SetUiState(ent.Owner, CP14TradingUiKey.Key, new CP14TradingPlatformUiState(GetNetEntity(user), GetNetEntity(ent), ent.Comp.NextBuyTime));
    }

    public bool TryUnlockPosition(Entity<CP14TradingReputationComponent?> user, ProtoId<CP14TradingPositionPrototype> position)
    {
        if (!CanUnlockPosition(user, position))
            return false;

        if (!_proto.TryIndex(position, out var indexedPosition))
            return false;

        if (!Resolve(user.Owner, ref user.Comp, false))
            return false;

        user.Comp.Reputation[indexedPosition.Faction] -= indexedPosition.UnlockReputationCost;
        user.Comp.UnlockedPositions.Add(position);
        Dirty(user);

        return true;
    }

    public bool TryBuyPosition(Entity<CP14TradingReputationComponent?> user, Entity<CP14TradingPlatformComponent> platform, ProtoId<CP14TradingPositionPrototype> position)
    {
        if (!CanBuyPosition(user, platform!, position))
            return false;

        if (!_proto.TryIndex(position, out var indexedPosition))
            return false;

        if (!Resolve(user.Owner, ref user.Comp, false))
            return false;

        platform.Comp.NextBuyTime = _timing.CurTime + indexedPosition.Cooldown;
        Dirty(platform);

        indexedPosition.Service.Buy(EntityManager, _proto, platform);
        return true;
    }

    public bool CanUnlockPosition(Entity<CP14TradingReputationComponent?> user, ProtoId<CP14TradingPositionPrototype> position)
    {
        if (!Resolve(user.Owner, ref user.Comp, false))
            return false;

        if (!_proto.TryIndex(position, out var indexedPosition))
            return false;

        if (user.Comp.UnlockedPositions.Contains(position))
            return false;

        if (indexedPosition.Prerequisite is not null && !user.Comp.UnlockedPositions.Contains(indexedPosition.Prerequisite.Value))
            return false;

        return user.Comp.Reputation.GetValueOrDefault(indexedPosition.Faction, 0f) >= indexedPosition.UnlockReputationCost;
    }

    public bool CanBuyPosition(Entity<CP14TradingReputationComponent?> user, Entity<CP14TradingPlatformComponent?> platform, ProtoId<CP14TradingPositionPrototype> position)
    {
        if (!Resolve(user.Owner, ref user.Comp, false))
            return false;
        if (!Resolve(platform.Owner, ref platform.Comp, false))
            return false;

        if (!user.Comp.UnlockedPositions.Contains(position))
            return false;

        if (_timing.CurTime < platform.Comp.NextBuyTime)
            return false;

        return true;
    }
}

[Serializable, NetSerializable]
public sealed class CP14TradingPositionUnlockAttempt(ProtoId<CP14TradingPositionPrototype> position) : BoundUserInterfaceMessage
{
    public readonly ProtoId<CP14TradingPositionPrototype> Position = position;
}

[Serializable, NetSerializable]
public sealed class CP14TradingPositionBuyAttempt(ProtoId<CP14TradingPositionPrototype> position) : BoundUserInterfaceMessage
{
    public readonly ProtoId<CP14TradingPositionPrototype> Position = position;
}
