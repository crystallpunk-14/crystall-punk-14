using System.Numerics;
using Content.Client._CP14.Input;
using Content.Client.Hands.Systems;
using Content.Client.Resources;
using Content.Client.UserInterface.Screens;
using Content.Shared._CP14.Fishing;
using Content.Shared._CP14.Fishing.Components;
using Content.Shared._CP14.Input;
using Content.Shared.Interaction;
using Robust.Client;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._CP14.Fishing;

public sealed class CP14FishingSystem : CP14SharedFishingSystem
{
    [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly InputSystem _input = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly HandsSystem _handsSystem = default!;

    private Popup? _fishingPopup;

    public override void Update(float dt)
    {
        base.Update(dt);

        if (!_gameTiming.IsFirstTimePredicted)
            return;

        var heldUid = _handsSystem.GetActiveHandEntity();

        if (!TryComp<CP14FishingRodComponent>(heldUid, out var fishingRodComponent))
            return;

        if (!fishingRodComponent.FishCaught)
            return;

        var reeling = _input.CmdStates.GetState(CP14ContentKeyFunctions.CP14FishingAction) == BoundKeyState.Down;
    }

    private void OpenFishingPopup(EntityUid uid, CP14FishingRodComponent component, AfterInteractEvent args)
    {
        // Getting data
        if (!_prototypeManager.Resolve(component.FishingMinigame, out var minigameStyle))
            return;

        var background = minigameStyle.Background;
        var floatUI = minigameStyle.Float;
        var progressbar = minigameStyle.Progressbar;
        var fishIcon = minigameStyle.FishIcon;

        // Generating popup
        _fishingPopup = new Popup
        {
            CloseOnClick = false,
            CloseOnEscape = false,
            MinSize = new Vector2(background.Size.X, background.Size.Y),
            MaxSize = new Vector2(background.Size.X, background.Size.Y),
        };

        var screenCenter = new Vector2();
        switch (_userInterfaceManager.ActiveScreen)
        {
            case DefaultGameScreen gameScreen:
                gameScreen.AddChild(_fishingPopup);
                screenCenter = gameScreen.Size / 2;
                break;

            case SeparatedChatGameScreen gameScreen:
                gameScreen.AddChild(_fishingPopup);
                screenCenter = gameScreen.Size / 2;
                break;
        }

        // Generating layers
        var backgroundTexture = _resourceCache.GetTexture(background.Texture);
        var firstLayer = new PanelContainer
        {
            PanelOverride = new StyleBoxTexture
            {
                Texture = backgroundTexture,
            },
            MinSize = new Vector2(background.Size.X, background.Size.Y),
            MaxSize = new Vector2(background.Size.X, background.Size.Y),
        };
        _fishingPopup.AddChild(firstLayer);

        var secondLayer = new PanelContainer
        {
            MinSize = new Vector2(background.Size.X, background.Size.Y),
            MaxSize = new Vector2(background.Size.X, background.Size.Y),
        };
        _fishingPopup.AddChild(secondLayer);

        // Filling first layer
        var progressbarTexture = _resourceCache.GetTexture(progressbar.Texture);
        var progressbarContainer = new PanelContainer
        {
            PanelOverride = new StyleBoxTexture
            {
                Texture = progressbarTexture,
            },
            MinSize = new Vector2(progressbar.Size.X, 0),
            SetSize = new Vector2(progressbar.Size.X, 0),
            MaxSize = new Vector2(progressbar.Size.X, progressbar.Size.Y),
            Margin = new Thickness(progressbar.Offset.X, 0, 0, progressbar.Offset.Y),
            HorizontalAlignment = Control.HAlignment.Left,
            VerticalAlignment = Control.VAlignment.Bottom,
        };
        firstLayer.AddChild(progressbarContainer);

        var floatTexture = _resourceCache.GetTexture(floatUI.Texture);
        var floatContainer = new PanelContainer
        {
            PanelOverride = new StyleBoxTexture
            {
                Texture = floatTexture,
            },
            MinSize = new Vector2(floatUI.Size.X, floatUI.Size.Y),
            MaxSize = new Vector2(floatUI.Size.X, floatUI.Size.Y),
            Margin = new Thickness(floatUI.Offset.X, 0, 0, floatUI.Offset.Y),
            HorizontalAlignment = Control.HAlignment.Left,
            VerticalAlignment = Control.VAlignment.Bottom,
        };
        firstLayer.AddChild(floatContainer);

        // Filling second layer
        var fishTexture = _resourceCache.GetTexture(fishIcon.Texture);
        var fishIconContainer = new PanelContainer
        {
            PanelOverride = new StyleBoxTexture
            {
                Texture = fishTexture,
            },
            MinSize = new Vector2(fishIcon.Size.X, fishIcon.Size.Y),
            MaxSize = new Vector2(fishIcon.Size.X, fishIcon.Size.Y),
            Margin = new Thickness(fishIcon.Offset.X, 0, 0, fishIcon.Offset.Y),
            HorizontalAlignment = Control.HAlignment.Left,
            VerticalAlignment = Control.VAlignment.Bottom,
        };
        secondLayer.AddChild(fishIconContainer);

        _fishingPopup.Open(UIBox2.FromDimensions(new Vector2(screenCenter.X - background.Size.X,
                screenCenter.Y - background.Size.Y),
                background.Size));
    }

    private void CloseFishingPopup()
    {
        if (_fishingPopup is null)
            return;

        switch (_userInterfaceManager.ActiveScreen)
        {
            case DefaultGameScreen gameScreen:
                gameScreen.RemoveChild(_fishingPopup);
                break;

            case SeparatedChatGameScreen gameScreen:
                gameScreen.RemoveChild(_fishingPopup);
                break;
        }

        _fishingPopup = null;
    }
}
