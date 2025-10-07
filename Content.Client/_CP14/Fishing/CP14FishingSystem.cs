using Content.Client.Hands.Systems;
using Content.Shared._CP14.Fishing;
using Content.Shared._CP14.Fishing.Components;
using Content.Shared._CP14.Input;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Input;
using Robust.Shared.Timing;

namespace Content.Client._CP14.Fishing;

public sealed class CP14FishingSystem : CP14SharedFishingSystem
{
    [Dependency] private readonly InputSystem _input = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14FishingRodComponent, AfterAutoHandleStateEvent>(OnFishingRodState);
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        if (!_gameTiming.IsFirstTimePredicted)
            return;

        var heldUid = _hands.GetActiveHandEntity();

        if (!TryComp<CP14FishingRodComponent>(heldUid, out var fishingRodComponent))
            return;

        if (_playerManager.LocalSession?.AttachedEntity is not { } player)
            return;

        UpdatePressedButtons(fishingRodComponent, player);
    }

    private void OnFishingRodState(Entity<CP14FishingRodComponent> entity, ref AfterAutoHandleStateEvent args)
    {
        if (_userInterface.TryGetOpenUi(entity.Owner, CP14FishingUiKey.Key, out var bui))
        {
            bui.Update();
        }
    }

    private void UpdatePressedButtons(CP14FishingRodComponent fishingRodComponent, EntityUid player)
    {
        if (fishingRodComponent.CaughtFish is null)
            return;

        var reelKey = _input.CmdStates.GetState(CP14ContentKeyFunctions.CP14FishingAction) == BoundKeyState.Down;

        if (fishingRodComponent.Reeling == reelKey)
            return;

        fishingRodComponent.Reeling = reelKey;
        RaiseNetworkEvent(new CP14FishingReelKeyMessage(reelKey));
    }
}
