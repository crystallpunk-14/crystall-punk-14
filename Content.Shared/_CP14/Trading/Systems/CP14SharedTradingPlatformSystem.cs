using Content.Shared._CP14.Trading.Components;
using Content.Shared._CP14.Trading.Prototypes;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.Trading.Systems;

public abstract partial class CP14SharedTradingPlatformSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = default!;
    [Dependency] protected readonly IPrototypeManager Proto = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeUI();

        SubscribeLocalEvent<CP14TradingReputationComponent, MapInitEvent>(OnReputationMapInit);
        SubscribeLocalEvent<CP14TradingContractComponent, UseInHandEvent>(OnContractUse);
    }

    private void OnReputationMapInit(Entity<CP14TradingReputationComponent> ent, ref MapInitEvent args)
    {
        foreach (var faction in Proto.EnumeratePrototypes<CP14TradingFactionPrototype>())
        {
            if (faction.RoundStart is not null)
            {
                ent.Comp.Reputation[faction] = ent.Comp.Reputation.GetValueOrDefault(faction, 0f) + faction.RoundStart.Value;
            }
        }
        Dirty(ent);
    }

    private void OnContractUse(Entity<CP14TradingContractComponent> ent, ref UseInHandEvent args)
    {
        if (!Proto.TryIndex(ent.Comp.Faction, out var indexedFaction))
            return;

        var repComp = EnsureComp<CP14TradingReputationComponent>(args.User);
        repComp.Reputation.TryAdd(ent.Comp.Faction, ent.Comp.StartReputation);
        _popup.PopupPredicted(Loc.GetString("cp14-trading-contract-use", ("name", Loc.GetString(indexedFaction.Name))), args.User, args.User);
        if (_net.IsServer)
            QueueDel(ent);
    }

    protected void UpdateUIState(Entity<CP14TradingPlatformComponent> ent, EntityUid user)
    {
        if (!TryComp<CP14TradingReputationComponent>(user, out var repComp))
            return;

        _userInterface.SetUiState(ent.Owner, CP14TradingUiKey.Key, new CP14TradingPlatformUiState(GetNetEntity(user), GetNetEntity(ent)));
    }

    public bool TryUnlockPosition(Entity<CP14TradingReputationComponent?> user, ProtoId<CP14TradingPositionPrototype> position)
    {
        if (!CanUnlockPosition(user, position))
            return false;

        if (!Proto.TryIndex(position, out var indexedPosition))
            return false;

        if (!Resolve(user.Owner, ref user.Comp, false))
            return false;

        user.Comp.Reputation[indexedPosition.Faction] -= indexedPosition.UnlockReputationCost;
        user.Comp.UnlockedPositions.Add(position);
        Dirty(user);

        return true;
    }

    public bool CanUnlockPosition(Entity<CP14TradingReputationComponent?> user, ProtoId<CP14TradingPositionPrototype> position)
    {
        if (!Resolve(user.Owner, ref user.Comp, false))
            return false;

        if (!Proto.TryIndex(position, out var indexedPosition))
            return false;

        if (!user.Comp.Reputation.ContainsKey(indexedPosition.Faction))
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

        if (Timing.CurTime < platform.Comp.NextBuyTime)
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
