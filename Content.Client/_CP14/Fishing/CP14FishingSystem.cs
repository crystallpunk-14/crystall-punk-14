using System.Diagnostics;
using System.Numerics;
using Content.Client.Resources;
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

        var fishingPopup = new Popup
        {
            CloseOnClick = false,
            CloseOnEscape = false,
            MinSize = new Vector2(background.Size.X, background.Size.Y),
            MaxSize = new Vector2(background.Size.X, background.Size.Y),
        };
        _userInterfaceManager.ModalRoot.AddChild(fishingPopup);

        var backgroundTexture = _resourceCache.GetTexture(background.Texture);
        var panel = new PanelContainer
        {
            PanelOverride = new StyleBoxTexture
            {
                Texture = backgroundTexture,
            },
            MinSize = new Vector2(background.Size.X, background.Size.Y),
            MaxSize = new Vector2(background.Size.X, background.Size.Y),
        };
        fishingPopup.AddChild(panel);

        fishingPopup.Open(UIBox2.FromDimensions(new Vector2(0, 0), background.Size));
    }
}
