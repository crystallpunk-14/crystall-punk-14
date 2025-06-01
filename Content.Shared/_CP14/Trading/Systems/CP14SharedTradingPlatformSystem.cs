using Content.Shared._CP14.Trading.Components;
using Content.Shared._CP14.Trading.Prototypes;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
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
    [Dependency] private readonly SharedAudioSystem _audio = default!;

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
        if (args.Handled)
            return;
        if (!Proto.TryIndex(ent.Comp.Faction, out var indexedFaction))
            return;

        args.Handled = true;

        var repComp = EnsureComp<CP14TradingReputationComponent>(args.User);
        repComp.Reputation.TryAdd(ent.Comp.Faction, 0);
        _audio.PlayLocal(new SoundCollectionSpecifier("CP14CoinImpact"), args.User, args.User);
        _popup.PopupPredicted(Loc.GetString("cp14-trading-contract-use", ("name", Loc.GetString(indexedFaction.Name))), args.User, args.User);

        if (_net.IsServer)
            QueueDel(ent);
    }

    protected void UpdateUIState(Entity<CP14TradingPlatformComponent> ent, EntityUid user)
    {
        if (!TryComp<CP14TradingReputationComponent>(user, out var repComp))
            return;

        _userInterface.SetUiState(ent.Owner, CP14TradingUiKey.Key, new CP14TradingPlatformUiState(GetNetEntity(ent)));
    }

    public bool CanBuyPosition(Entity<CP14TradingReputationComponent?> user, ProtoId<CP14TradingPositionPrototype> position)
    {
        if (!Resolve(user.Owner, ref user.Comp, false))
            return false;
        if (!Proto.TryIndex(position, out var indexedPosition))
            return false;

        if (user.Comp.Reputation[indexedPosition.Faction] < indexedPosition.ReputationLevel)
            return false;

        return true;
    }
}

[Serializable, NetSerializable]
public sealed class CP14TradingPositionBuyAttempt(ProtoId<CP14TradingPositionPrototype> position) : BoundUserInterfaceMessage
{
    public readonly ProtoId<CP14TradingPositionPrototype> Position = position;
}
