using System.Diagnostics.CodeAnalysis;
using Content.Client.Hands.Systems;
using Content.Shared._CP14.Fishing;
using Content.Shared._CP14.Fishing.Components;
using Content.Shared._CP14.Input;
using Robust.Client.GameObjects;
using Robust.Shared.Input;

namespace Content.Client._CP14.Fishing;

public sealed class CP14FishingSystem : CP14SharedFishingSystem
{
    [Dependency] private readonly InputSystem _input = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14FishingRodComponent, AfterAutoHandleStateEvent>(OnFishingRodState);
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        var heldUid = _hands.GetActiveHandEntity();

        if (!TryComp<CP14FishingRodComponent>(heldUid, out var fishingRodComponent))
            return;

        UpdatePressedButtons(fishingRodComponent);
    }

    /// <summary>
    /// Handles BUI updates
    /// </summary>
    private void OnFishingRodState(Entity<CP14FishingRodComponent> entity, ref AfterAutoHandleStateEvent args)
    {
        if (_userInterface.TryGetOpenUi(entity.Owner, CP14FishingUiKey.Key, out var bui))
        {
            bui.Update();
        }
    }

    /// <summary>
    /// Handles user inputs
    /// </summary>
    private void UpdatePressedButtons(CP14FishingRodComponent fishingRodComponent)
    {
        if (fishingRodComponent.CaughtFish is null)
            return;

        var reelKey = _input.CmdStates.GetState(CP14ContentKeyFunctions.CP14FishingAction) == BoundKeyState.Down;

        if (fishingRodComponent.Reeling == reelKey)
            return;

        fishingRodComponent.Reeling = reelKey;
        RaiseNetworkEvent(new CP14FishingReelKeyMessage(reelKey));
    }

    /// <summary>
    /// Used to get fish and rod component by FishingUI
    /// </summary>
    /// <returns>True when all info can be resolved</returns>
    public bool GetInfo([NotNullWhen(true)] out CP14FishingRodComponent? rodComponent,
        [NotNullWhen(true)] out CP14FishComponent? fishComponent)
    {
        rodComponent = null;
        fishComponent = null;

        var heldUid = _hands.GetActiveHandEntity();

        if (!TryComp<CP14FishingRodComponent>(heldUid, out var posRodComponent))
            return false;

        if (!TryComp<CP14FishComponent>(posRodComponent.CaughtFish, out var posFishComponent))
            return false;

        rodComponent = posRodComponent;
        fishComponent = posFishComponent;
        return true;
    }
}
