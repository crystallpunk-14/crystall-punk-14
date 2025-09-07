using System.Numerics;
using Content.Client.Resources;
using Content.Client.UserInterface.Screens;
using Content.Shared._CP14.Fishing;
using Content.Shared._CP14.Fishing.Components;
using Content.Shared.Interaction;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;

namespace Content.Client._CP14.Fishing;

public sealed class CP14FishingSystem : CP14SharedFishingSystem
{
    [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14FishingRodComponent, AfterInteractEvent>(OnInteract);
    }

    private void OnInteract(EntityUid uid, CP14FishingRodComponent component, AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        if (!args.CanReach || args.Target is not { Valid: true })
            return;

        if (!TryComp<CP14FishingPondComponent>(args.Target, out var pond))
            return;

        if (component.FishingProcess)
            return;

        OpenFishingPopup(uid, component, args);
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
        var fishingPopup = new Popup
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
                gameScreen.AddChild(fishingPopup);
                screenCenter = gameScreen.Size / 2;
                break;

            case SeparatedChatGameScreen gameScreen:
                gameScreen.AddChild(fishingPopup);
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
        fishingPopup.AddChild(firstLayer);

        var secondLayer = new PanelContainer
        {
            MinSize = new Vector2(background.Size.X, background.Size.Y),
            MaxSize = new Vector2(background.Size.X, background.Size.Y),
        };
        fishingPopup.AddChild(secondLayer);

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
            VerticalAlignment = Control.VAlignment.Bottom
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
            VerticalAlignment = Control.VAlignment.Bottom
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
            VerticalAlignment = Control.VAlignment.Bottom
        };
        secondLayer.AddChild(fishIconContainer);

        fishingPopup.Open(UIBox2.FromDimensions(new Vector2(screenCenter.X * 0.85f, screenCenter.Y * 0.65f), background.Size));
    }
}
