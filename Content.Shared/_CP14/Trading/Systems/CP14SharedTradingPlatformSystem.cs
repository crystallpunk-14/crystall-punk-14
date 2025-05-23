using Content.Shared._CP14.Trading.Components;
using Content.Shared.UserInterface;

namespace Content.Shared._CP14.Trading.Systems;

public abstract partial class CP14SharedTradingPlatformSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14TradingPlatformComponent, BeforeActivatableUIOpenEvent>(OnBeforeUIOpen);
    }

    private void OnBeforeUIOpen(Entity<CP14TradingPlatformComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        if (!TryComp<CP14TradingReputationComponent>(args.User, out var repComp))
            return;

        _userInterface.SetUiState(ent.Owner, CP14TradingUiKey.Key, new CP14TradingPlatformUiState(repComp.Reputation, repComp.UnlockedPositions));
    }
}
